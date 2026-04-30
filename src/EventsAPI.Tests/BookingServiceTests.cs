using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using RU.Uncio.EventsAPI.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace EventsAPI.Tests
{
    public class BookingServiceTests
    {
        private readonly Mock<ILogger<BookingService>> bookingsLogger;
        private readonly EventsService eventsService;
        private readonly Dictionary<Guid, Event> events;

        public BookingServiceTests()
        {
            bookingsLogger = new Mock<ILogger<BookingService>>();
            var mockRepository = new Mock<IEventRepository>();
            var logger = new Mock<ILogger<EventsService>>();
            eventsService = new EventsService(logger.Object, mockRepository.Object);
            events = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                }
                .ToDictionary(ev => ev.Id, events => events);

            mockRepository.Setup(method => method.GetEvents()).Returns(events);
        }

        [Fact]
        public async Task AddBookingForExistingEvent_ReturnsPendingBooking()
        {
            //Arrange
            var eventToBook = events.FirstOrDefault().Value;
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking = await bookingServiceToAdd.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatus.Pending, newBooking.Status);
            Assert.Equal(eventToBook.Id, newBooking.EventId);
        }

        [Fact]
        public async Task AddSeveralBookingsForSameExistingEvent_ReturnsBookingsWithDifferentIdsNotEmpty()
        {
            //Arrange
            var eventToBook1 = events.FirstOrDefault().Value;
            var eventToBook2 = events.LastOrDefault().Value;

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking1 = await bookingServiceToAdd.CreateBookingAsync(eventToBook1.Id, TestContext.Current.CancellationToken);
            var newBooking2 = await bookingServiceToAdd.CreateBookingAsync(eventToBook1.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotEqual(new Guid(), newBooking1.Id);
            Assert.NotEqual(new Guid(), newBooking2.Id);
            Assert.NotEqual(newBooking1.Id, newBooking2.Id);
        }

        [Fact]
        public async Task GetBookingById_ReturnsExpectingBooking()
        {
            //Arrange
            var eventToBook = events.FirstOrDefault().Value;

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService);
            var initialBookings = new Dictionary<Guid, Booking>();


            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
            var result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newBooking.Id, result.Id);
            Assert.Equal(eventToBook.Id, result.EventId);
        }

        [Fact]
        public async Task UpdateBookingStatus_UpdatesExpectingBooking()
        {
            //Arrange
            var eventToBook = events.FirstOrDefault().Value;

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            bookingRepoMock.Setup(method => method.UpdateBookingAsync(It.IsAny<Guid>(), It.IsAny<BookingStatus>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, BookingStatus, CancellationToken>((id, status, token) => initialBookings[id].Status = status);

            // Act
            var newBooking = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
            var result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            var resultBefore = result.Status;

            await bookingRepoMock.Object.UpdateBookingAsync(newBooking.Id, BookingStatus.Confirmed, TestContext.Current.CancellationToken);
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
            var bookingService = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);

            var id = Guid.NewGuid();

            // Act
            var result = await bookingService.GetBookingByIdAsync(id, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddEventBooking_WhenEventDoesntExist_ThrowsMissingEvent()
        {
            //Arrange
            var eventToBook = new Event("Test", DateTime.Now, DateTime.Now + TimeSpan.FromHours(4));
            var expectedExceptionMessage = $"Event with ID {eventToBook.Id} is not found in the collection";
           
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception =await Assert
                .ThrowsAsync<MissingEventException>(async () => await bookingServiceToAdd.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddEventBooking_WhenEventRemoved_ThrowsMissingEvent()
        {
            //Arrange
            var mockRepositoryLocal = new Mock<IEventRepository>();
            var loggerLocal = new Mock<ILogger<EventsService>>();
            var eventsServiceLocal = new EventsService(loggerLocal.Object, mockRepositoryLocal.Object);
            var eventsLocal = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepositoryLocal.Setup(method => method.GetEvents()).Returns(eventsLocal);
            mockRepositoryLocal.Setup(method => method.RemoveEvent(It.IsAny<Guid>())).Callback<Guid>((guid) => eventsLocal.Remove(guid));

            var eventToRemove = eventsLocal.LastOrDefault().Value;
            eventsServiceLocal.RemoveEvent(eventToRemove.Id);

            var expectedExceptionMessage = $"Event with ID {eventToRemove.Id} is not found in the collection";

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsServiceLocal);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = await Assert
                .ThrowsAsync<MissingEventException>(async () => await bookingServiceToAdd.CreateBookingAsync(eventToRemove.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }
    }
}
