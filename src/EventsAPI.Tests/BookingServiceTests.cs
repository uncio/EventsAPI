using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using RU.Uncio.EventsAPI.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventsAPI.Tests
{
    public class BookingServiceTests
    {
        private readonly Mock<ILogger<BookingService>> bookingsLogger;
        private readonly CancellationTokenSource cancellationToken;

        public BookingServiceTests()
        {
            bookingsLogger = new Mock<ILogger<BookingService>>();
            cancellationToken = new CancellationTokenSource();
        }

        [Fact]
        public async Task AddBookingForExistingEvent_ReturnsPendingBooking()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookings()).Returns(initialBookings);
            bookingRepoMock.Setup<bool>(method => method.AddBooking(It.IsAny<Booking>())).Callback<Booking>((b) => initialBookings.Add(b.Id, b)).Returns(true);

            var eventIdToBook = Guid.NewGuid();

            // Act
            var newBooking = await bookingServiceToAdd.CreateBookingAsync(eventIdToBook);

            // Assert
            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatus.Pending, newBooking.Status);
            Assert.Equal(eventIdToBook, newBooking.EventId);
        }

        [Fact]
        public async Task AddSeveralBookingsForSameExistingEvent_ReturnsBookingsWithDifferentIdsNotEmpty()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();


            bookingRepoMock.Setup(method => method.GetBookings()).Returns(initialBookings);
            bookingRepoMock.Setup<bool>(method => method.AddBooking(It.IsAny<Booking>())).Callback<Booking>((b) => initialBookings.Add(b.Id, b)).Returns(true);

            var eventIdToBook1 = Guid.NewGuid();
            var eventIdToBook2 = Guid.NewGuid();

            // Act
            var newBooking1 = await bookingServiceToAdd.CreateBookingAsync(eventIdToBook1);
            var newBooking2 = await bookingServiceToAdd.CreateBookingAsync(eventIdToBook2);

            // Assert
            Assert.NotEqual(new Guid(), newBooking1.Id);
            Assert.Equal(newBooking1.Id, newBooking1.Id);
        }

        [Fact]
        public async Task GetBookingById_ReturnsExpectingBooking()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();


            bookingRepoMock.Setup(method => method.GetBookings()).Returns(initialBookings);
            bookingRepoMock.Setup<bool>(method => method.AddBooking(It.IsAny<Booking>())).Callback<Booking>((b) => initialBookings.Add(b.Id, b)).Returns(true);

            var eventIdToBook = Guid.NewGuid();

            // Act
            var newBooking = await bookingServiceToAdd.CreateBookingAsync(eventIdToBook);
            var result = await bookingServiceToAdd.GetBookingByIdAsync(newBooking.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newBooking.Id, result.Id);
            Assert.Equal(eventIdToBook, result.EventId);
        }

        [Fact]
        public async Task GetBookingWithStatusChangedIn10Sec_Success()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookings()).Returns(initialBookings);
            bookingRepoMock.Setup(method => method.AddBooking(It.IsAny<Booking>())).Callback<Booking>((b) => initialBookings.Add(b.Id, b)).Returns(true);
            bookingRepoMock.Setup(method => method.UpdateBooking(It.IsAny<Guid>(), It.IsAny<BookingStatus>())).Callback<Guid, BookingStatus>((guid, st)
                => initialBookings[guid].Status = st);

            var eventIdToBook = Guid.NewGuid();

            // Act
            await bookingServiceToAdd.StartAsync(cancellationToken.Token);
            var newBooking = await bookingServiceToAdd.CreateBookingAsync(eventIdToBook);
            var resultBefore = newBooking.Status;

            await Task.Delay(TimeSpan.FromSeconds(10));

            var resultAfter = newBooking.Status;

            // Assert
            Assert.Equal(BookingStatus.Pending, resultBefore);
            Assert.NotEqual(BookingStatus.Pending, resultAfter);
        }

        [Fact]
        public async Task GetBookingById_WhenIdDoesntExist_ReturnsNull()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookings()).Returns(initialBookings);

            var id = Guid.NewGuid();
            // Act
            var result = await bookingServiceToAdd.GetBookingByIdAsync(id);

            // Assert
            Assert.Null(result);
        }
    }
}
