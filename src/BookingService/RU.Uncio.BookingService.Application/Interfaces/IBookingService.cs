using RU.Uncio.BookingService.Domain.Models;

namespace RU.Uncio.BookingService.Application.Interfaces
{
    /// <summary>
    /// Bookings manipulation service interface
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// Creates a booking asynchronously
        /// </summary>
        /// <param name="userId">user id for the new booking</param>
        /// <param name="eventId">event id of the new booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Booking> CreateBookingAsync(Guid userId, Guid eventId, CancellationToken token);
        /// <summary>
        /// Gets a booking asynchronously by booking ID
        /// </summary>
        /// <param name="bookingId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken token);

        /// <summary>
        /// Cancels a booking for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="bookingId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task CancelBookingByIdAsync(Guid userId, string userRole, Guid bookingId, CancellationToken token);
    }
}
