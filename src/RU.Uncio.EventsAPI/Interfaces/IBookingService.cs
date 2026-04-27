using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Interfaces
{
    /// <summary>
    /// Bookings manipulation service interface
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// Creates a booking asynchronously
        /// </summary>
        /// <param name="eventId">event id of the new booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken token);
        /// <summary>
        /// Gets a booking asynchronously by booking ID
        /// </summary>
        /// <param name="bookingId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken token);
    }
}
