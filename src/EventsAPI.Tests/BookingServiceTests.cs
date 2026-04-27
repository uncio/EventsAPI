using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
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

            bookingRepoMock.Setup(method => method.GetBookingsAsync()).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>())).Callback<Booking>((b) => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

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


            bookingRepoMock.Setup(method => method.GetBookingsAsync()).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>())).Callback<Booking>((b) => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

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


            bookingRepoMock.Setup(method => method.GetBookingsAsync()).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>())).Callback<Booking>((b) => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

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

            bookingRepoMock.Setup(method => method.GetBookingsAsync()).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup(method => method.AddBookingAsync(It.IsAny<Booking>())).Callback<Booking>((b) => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            bookingRepoMock.Setup(method => method.UpdateBookingAsync(It.IsAny<Guid>(), It.IsAny<BookingStatus>())).Callback<Guid, BookingStatus>((guid, st)
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

            bookingRepoMock.Setup(method => method.GetBookingsAsync()).ReturnsAsync(initialBookings);

            var id = Guid.NewGuid();
            // Act
            var result = await bookingServiceToAdd.GetBookingByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddEventBooking_WhenEventDoesntExist_ReturnsNotFound()
        {
            var mockRepository = new Mock<IEventRepository>();
            var logger = new Mock<ILogger<EventsService>>();
            var eventsService = new EventsService(logger.Object, mockRepository.Object);
            var events = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepository.Setup(method => method.GetEvents()).Returns(events);

            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            var result = await client.PostAsync($"/{Guid.NewGuid()}/book", null, cancellationToken.Token);

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task AddEventBooking_WhenEventRemoved_ReturnsNotFound()
        {
            var mockRepository = new Mock<IEventRepository>();
            var logger = new Mock<ILogger<EventsService>>();
            var eventsService = new EventsService(logger.Object, mockRepository.Object);
            var events = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepository.Setup(method => method.GetEvents()).Returns(events);
            mockRepository.Setup(method => method.RemoveEvent(It.IsAny<Guid>())).Callback<Guid>((guid) => events.Remove(guid));

            var eventToRemove = events.LastOrDefault();
            eventsService.RemoveEvent(eventToRemove.Key);

            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            var result = await client.PostAsync($"/{eventToRemove.Key}/book", null, cancellationToken.Token);

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}
