using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
{
    public class BookingService : BackgroundService, IBookingService
    {
        public Task CreateBookingAsync(Guid eventId)
        {
            throw new NotImplementedException();
        }

        public Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            throw new NotImplementedException();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
