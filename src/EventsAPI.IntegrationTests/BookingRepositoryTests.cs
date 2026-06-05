using Microsoft.EntityFrameworkCore;
using RU.Uncio.EventsAPI.DataAccess;
using RU.Uncio.EventsAPI.Models;
using RU.Uncio.EventsAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EventsAPI.IntegrationTests
{
    public class BookingRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.Migrate();
            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE events, bookings RESTART IDENTITY CASCADE");
        }

        [Fact]
        public async Task AddBooking_Success()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new BookingRepository(context);
            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var booking = new Booking(ev.Id);
            // Act
            await repository.AddBookingAsync(booking, t.Token);

            // Assert — через отдельный контекст
            await using var verifyContext = CreateContext();
            var verifyRepository = new BookingRepository(verifyContext);
            var verifyBookings = await verifyRepository.GetBookingsAsync(t.Token);
            var saved = verifyBookings.Values.FirstOrDefault(b => b.Id == booking.Id);

            Assert.NotNull(saved);
            Assert.Equal(ev.Id, saved.EventId);
        }

        [Fact]
        public async Task UpdateBooking_Success()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new BookingRepository(context);
            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var booking = new Booking(ev.Id);
            await repository.AddBookingAsync(booking, t.Token);

            // Act
            await using var actContext = CreateContext();
            var actRepository = new BookingRepository(actContext);
            var actBookings = await actRepository.GetBookingsAsync(t.Token);
            var actBooking = actBookings.Values.FirstOrDefault(b => b.Id == booking.Id);
            actBooking?.Confirm();
            await actRepository.UpdateBookingAsync(actBooking!, t.Token);

            // Assert
            await using var verifyContext = CreateContext();
            var verifyRepository = new BookingRepository(verifyContext);
            var verifyBookings = await verifyRepository.GetBookingsAsync(t.Token);
            var updated = verifyBookings.Values.FirstOrDefault(b => b.Id == booking.Id);
            Assert.Equal(BookingStatus.Confirmed, updated?.Status);
        }

        [Fact]
        public async Task GetAllBookings_ReturnsFullCollection()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new BookingRepository(context);
            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var booking = new Booking(ev.Id);
            await repository.AddBookingAsync(booking, t.Token);

            var booking2 = new Booking(ev.Id);
            await repository.AddBookingAsync(booking2, t.Token);

            // Act
            await using var actContext = CreateContext();
            var actRepository = new BookingRepository(actContext);
            var result = await actRepository.GetBookingsAsync(t.Token);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(ev.Id, result.Values.First().EventId);
            Assert.Equal(ev.Id, result.Values.Last().EventId);
        }

        [Fact]
        public async Task GetPendingBookings_ReturnsOnlyPending()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new BookingRepository(context);
            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var booking = new Booking(ev.Id);
            await repository.AddBookingAsync(booking, t.Token);

            var booking2 = new Booking(ev.Id);
            await repository.AddBookingAsync(booking2, t.Token);

            booking.Confirm();
            await repository.UpdateBookingAsync(booking, t.Token);
            // Act
            await using var actContext = CreateContext();
            var actRepository = new BookingRepository(actContext);
            var result = await actRepository.GetPendingBookingsAsync(t.Token);

            // Assert
            Assert.Single(result);
            Assert.All(result, r => Assert.Equal(BookingStatus.Pending, r.Status));
        }
    }
}
