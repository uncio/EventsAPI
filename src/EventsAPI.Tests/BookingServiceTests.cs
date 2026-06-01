using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RU.Uncio.EventsAPI.DataAccess;
using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using RU.Uncio.EventsAPI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace EventsAPI.Tests
{
    public class BookingServiceTests
    {
        private readonly IEventsService eventsService;
        private readonly IBookingService bookingService;
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;

        public BookingServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventsService, EventsService>();
            services.AddScoped<IBookingService, BookingService>();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();
            eventsService = serviceScope.ServiceProvider.GetRequiredService<IEventsService>();
            bookingService = serviceScope.ServiceProvider.GetRequiredService<IBookingService>();
        }

        [Fact]
        public async Task AddBookingForExistingEvent_ReturnsPendingBooking()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            // Act
            var newBooking = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatus.Pending, newBooking.Status);
            Assert.Equal(eventToBook.Id, newBooking.EventId);
        }

        [Fact]
        public async Task AddBookingForExistingEvent_DecreasesAvailableSeats()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            // Act
            var currentAvailableSeats = eventToBook.AvailableSeats;
            var newBooking = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
            var updatedAvailableSeats = eventToBook.AvailableSeats;

            // Assert
            Assert.Equal(currentAvailableSeats - 1, updatedAvailableSeats);
        }

        [Fact]
        public async Task AddSeveralBookingsForSameExistingEvent_ReturnsBookingsWithDifferentIdsNotEmpty()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            // Act
            var newBooking1 = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
            var newBooking2 = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotEqual(new Guid(), newBooking1.Id);
            Assert.NotEqual(new Guid(), newBooking2.Id);
            Assert.NotEqual(newBooking1.Id, newBooking2.Id);
        }

        [Fact]
        public async Task GetBookingById_ReturnsExpectingBooking()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            // Act
            var newBooking = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
            var result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newBooking.Id, result.Id);
            Assert.Equal(eventToBook.Id, result.EventId);
        }

        [Fact]
        public async Task GetBookingById_WhenIdDoesntExist_ReturnsNull()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            var id = Guid.NewGuid();

            // Act
            var newBooking = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
            var result = await bookingService.GetBookingByIdAsync(id, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddEventBooking_WhenEventDoesntExist_ThrowsMissingEvent()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            var id = Guid.NewGuid();
            var expectedExceptionMessage = $"Event with ID {id} is not found in the collection";

            // Act
            var exception = await Assert
                .ThrowsAsync<MissingEventException>(async () =>
                await bookingService.CreateBookingAsync(id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddEventBooking_WhenEventRemoved_ThrowsMissingEvent()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            var expectedExceptionMessage = $"Event with ID {eventToBook.Id} is not found in the collection";
            await eventsService.RemoveEventAsync(eventToBook.Id);
            // Act
            var exception = await Assert
                .ThrowsAsync<MissingEventException>(async () =>
                await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddBookingsForSameExistingEvent_WhenOverAvailableSeats_ThrowsNoAvailableSeats()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 1);
            await eventsService.AddEventAsync(eventToBook);

            var expectedExceptionMessage = $"No available seats for this event";
            // Act
            var newBooking1 = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);

            var exception = await Assert
                .ThrowsAsync<NoAvailableSeatsException>(async () =>
                await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<NoAvailableSeatsException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task ConfirmBooking_ReturnsConfirmedStatusAndFilledProcessedAtAndDecreasesAvailableSeats()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            // Act
            var seatsBefore = eventToBook.AvailableSeats;
            var newBooking = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
            var timeBefore = newBooking.ProcessedAt;
            newBooking.Confirm();
            var timeAfter = newBooking.ProcessedAt;
            var seatsAfter = eventToBook.AvailableSeats;

            // Assert
            Assert.Equal(BookingStatus.Confirmed, newBooking.Status);
            Assert.Null(timeBefore);
            Assert.NotNull(timeAfter);
            Assert.Equal(seatsBefore, seatsAfter + 1);
        }

        [Fact]
        public async Task RejectBooking_ReturnsRejectedStatusAndFilledProcessedAtAndReleasesSeats()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            // Act
            var seatsBefore = eventToBook.AvailableSeats;
            var newBooking = await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
            var timeBefore = newBooking.ProcessedAt;

            newBooking.Reject();
            eventToBook.ReleaseSeats();

            var timeAfter = newBooking.ProcessedAt;
            var seatsAfter = eventToBook.AvailableSeats;

            // Assert
            Assert.Equal(BookingStatus.Rejected, newBooking.Status);
            Assert.Null(timeBefore);
            Assert.NotNull(timeAfter);
            Assert.Equal(seatsBefore, seatsAfter);
        }

        [Fact]
        public async Task AddSeveralBookingsSimultaneously_Success()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 5);
            await eventsService.AddEventAsync(eventToBook);

            // Act
            var tasks = Enumerable.Range(0, 20)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    try
                    {
                        await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }));

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(5, results.Where(x => x).Count());
            Assert.Equal(15, results.Where(x => !x).Count());
        }

        [Fact]
        public async Task AddSeveralBookingsSimultaneously_ReturnsBookingsWithUniqueIds()
        {
            //Arrange
            Event eventToBook = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            await eventsService.AddEventAsync(eventToBook);

            // Act
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    return await bookingService.CreateBookingAsync(eventToBook.Id, TestContext.Current.CancellationToken);
                }));

            var results = await Task.WhenAll(tasks);

            var uniqueBookings = results.GroupBy(b => b.Id);

            // Assert
            Assert.Equal(10, uniqueBookings.Count());
        }

    }
}
