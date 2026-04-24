using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(Guid eventId);
        Task<Booking> GetBookingByIdAsync(Guid bookingId);
    }
}
