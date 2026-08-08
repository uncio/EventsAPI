using Microsoft.EntityFrameworkCore;
using RU.Uncio.BookingService.Domain.Models;
using RU.Uncio.BookingService.Infrastructure.DataAccess;
using RU.Uncio.Infrastructure.Repositories;
using Testcontainers.PostgreSql;
using Xunit;

namespace RU.Uncio.BookingService.IntegrationTests
{
    public class BookingRepositoryTests : IAsyncLifetime
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
#pragma warning restore CS0618 // Type or member is obsolete
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
                "TRUNCATE TABLE bookings RESTART IDENTITY CASCADE");
        }

        [Fact]
        public async Task AddBooking_Success()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new BookingRepository(context);

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            await context.SaveChangesAsync();

            var booking = new Booking(userId, eventId);
            // Act
            await repository.AddBookingAsync(booking, t.Token);

            // Assert — через отдельный контекст
            await using var verifyContext = CreateContext();
            var verifyRepository = new BookingRepository(verifyContext);
            var verifyBookings = await verifyRepository.GetBookingsAsync(t.Token);
            var saved = verifyBookings.Values.FirstOrDefault(b => b.Id == booking.Id);

            Assert.NotNull(saved);
            Assert.Equal(eventId, saved.EventId);
        }

        [Fact]
        public async Task UpdateBooking_Success()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new BookingRepository(context);

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            await context.SaveChangesAsync();

            var booking = new Booking(userId, eventId);
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

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var booking = new Booking(userId, eventId);
            await repository.AddBookingAsync(booking, t.Token);

            var booking2 = new Booking(userId, eventId);
            await repository.AddBookingAsync(booking2, t.Token);

            // Act
            await using var actContext = CreateContext();
            var actRepository = new BookingRepository(actContext);
            var result = await actRepository.GetBookingsAsync(t.Token);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(eventId, result.Values.First().EventId);
            Assert.Equal(eventId, result.Values.Last().EventId);
        }

        [Fact]
        public async Task GetPendingBookings_ReturnsOnlyPending()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new BookingRepository(context);

            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var booking = new Booking(userId, eventId);
            await repository.AddBookingAsync(booking, t.Token);

            var booking2 = new Booking(userId, eventId);
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
