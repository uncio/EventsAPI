using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Interfaces
{
    public interface IBookingService
    {
        Task<Guid> CreateBookingAsync(Guid eventId);
        Task<Booking> GetBookingByIdAsync(Guid bookingId);
    }
}
