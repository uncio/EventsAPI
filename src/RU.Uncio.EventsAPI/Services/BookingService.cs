using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
{
    public class BookingService : BackgroundService, IBookingService
    {
        private readonly ILogger<BookingService> logger;
        private readonly IServiceScopeFactory scopeFactory;

        public BookingService(ILogger<BookingService> log, IServiceScopeFactory scope)
        {
            logger = log;
            scopeFactory = scope;
        }

        public async Task<Guid> CreateBookingAsync(Guid eventId)
        {
            using var scope = scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider
                .GetRequiredService<IBookingRepository>();
            var newBooking = new Booking(eventId);

            bookingService.AddBooking(newBooking);

            return newBooking.Id;
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            using var scope = scopeFactory.CreateScope();
            var bookingRepo = scope.ServiceProvider
                .GetRequiredService<IBookingRepository>();

            var bookings = bookingRepo.GetBookings();

            if (bookings.TryGetValue(bookingId, out var booking))
                return booking;

            logger.LogError($"Booking queue doesn't contain a booking with id {bookingId}");
            return null;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
