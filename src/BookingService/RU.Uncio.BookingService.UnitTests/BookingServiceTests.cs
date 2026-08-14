using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RU.Uncio.BookingService.Application.Interfaces;
using RU.Uncio.BookingService.Application.Services;
using RU.Uncio.BookingService.Domain.Exceptions;
using RU.Uncio.BookingService.Domain.Models;
using System.Runtime.InteropServices;

namespace RU.Uncio.BookingService.UnitTests
{
    public class BookingServiceTests
    {
        private readonly Mock<ILogger<BookService>> bookingsLogger;

        public BookingServiceTests()
        {
            bookingsLogger = new Mock<ILogger<BookService>>();
        }

        [Fact]
        public async Task AddBookingForExistingEvent_ReturnsPendingBooking()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking = await bookingServiceToAdd.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatus.Pending, newBooking.Status);
            Assert.Equal(eventId, newBooking.EventId);
            Assert.Equal(userId, newBooking.UserId);
        }

        
        [Fact]
        public async Task AddSeveralBookingsForSameExistingEvent_ReturnsBookingsWithDifferentIdsNotEmpty()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking1 = await bookingServiceToAdd.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);
            var newBooking2 = await bookingServiceToAdd.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotEqual(new Guid(), newBooking1.Id);
            Assert.NotEqual(new Guid(), newBooking2.Id);
            Assert.NotEqual(newBooking1.Id, newBooking2.Id);
        }

        [Fact]
        public async Task GetBookingById_ReturnsExpectingBooking()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking = await bookingService.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);
            var result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newBooking.Id, result.Id);
            Assert.Equal(eventId, result.EventId);
            Assert.Equal(userId, result.UserId);
        }

        [Fact]
        public async Task UpdateBookingStatus_UpdatesExpectingBooking()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            bookingRepoMock.Setup(method => method.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((booking, token) => booking.Confirm());

            // Act
            var newBooking = await bookingService.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);
            var result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            var resultBefore = result.Status;

            newBooking.Confirm();
            await bookingRepoMock.Object.UpdateBookingAsync(newBooking, TestContext.Current.CancellationToken);
            result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            var resultAfter = result.Status;

            // Assert
            Assert.Equal(BookingStatus.Pending, resultBefore);
            Assert.NotEqual(BookingStatus.Pending, resultAfter);
        }



        [Fact]
        public async Task GetBookingById_WhenIdDoesntExist_ReturnsNull()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);

            var id = Guid.NewGuid();

            // Act
            var result = await bookingService.GetBookingByIdAsync(id, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ConfirmBooking_ReturnsConfirmedStatusAndFilledProcessedAt()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            bookingRepoMock.Setup(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking = await bookingService.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);
            var timeBefore = newBooking.ProcessedAt;
            newBooking.Confirm();
            var timeAfter = newBooking.ProcessedAt;

            // Assert
            Assert.Equal(BookingStatus.Confirmed, newBooking.Status);
            Assert.Null(timeBefore);
            Assert.NotNull(timeAfter);
        }

        [Fact]
        public async Task RejectBooking_ReturnsRejectedStatusAndFilledProcessedAt()
        {
            //Arrange            
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            bookingRepoMock.Setup(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking = await bookingService.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);
            var timeBefore = newBooking.ProcessedAt;

            newBooking.Reject();

            var timeAfter = newBooking.ProcessedAt;

            // Assert
            Assert.Equal(BookingStatus.Rejected, newBooking.Status);
            Assert.Null(timeBefore);
            Assert.NotNull(timeAfter);
        }
        [Fact]
        public async Task AdminCancellsAnyBooking_Succeeded()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userRole = "Admin";

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            bookingRepoMock.Setup(method => method.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((booking, token) => booking.Cancell());

            var newBooking = await bookingServiceToAdd.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);
            // Act

            await bookingServiceToAdd.CancelBookingByIdAsync(userId, userRole, newBooking.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatus.Cancelled, newBooking.Status);
        }

        [Fact]
        public async Task UserCancellsOwnBooking_Succeeded()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userRole = "User";

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            bookingRepoMock.Setup(method => method.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((booking, token) => booking.Cancell());

            var newBooking = await bookingServiceToAdd.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);
            // Act

            await bookingServiceToAdd.CancelBookingByIdAsync(userId, userRole, newBooking.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatus.Cancelled, newBooking.Status);
        }

        [Fact]
        public async Task UserCancellsSomeonesBooking_ThrowsNoRights()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var userRole = "User";

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            bookingRepoMock.Setup(method => method.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((booking, token) => booking.Cancell());

            var newBooking = await bookingServiceToAdd.CreateBookingAsync(userId, eventId, TestContext.Current.CancellationToken);

            var expectedExceptionMessage = $"User with ID {userId2} can't cancell the booking with ID {newBooking.Id}";

            // Act
            var exception = await Assert
                .ThrowsAsync<NoRightsException>(async () => await bookingServiceToAdd.CancelBookingByIdAsync(userId2, userRole, newBooking.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<NoRightsException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }
    }
}
