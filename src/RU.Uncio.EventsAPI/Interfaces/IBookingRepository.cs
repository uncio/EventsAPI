using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Interfaces
{
    /// <summary>
    /// Bookings Repository wrapper
    /// </summary>
    public interface IBookingRepository
    {
        /// <summary>
        /// Gets all bookings from collection
        /// </summary>
        /// <param name="token"></param>
        /// <returns>collection of existing bookings</returns>
        Task<Dictionary<Guid, Booking>> GetBookingsAsync(CancellationToken token);
        /// <summary>
        /// Adds a booking to collection
        /// </summary>
        /// <param name="book">booking to add</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> AddBookingAsync(Booking book, CancellationToken token);
        /// <summary>
        /// Updates a booking status in collection by booking ID
        /// </summary>
        /// <param name="id">booking id</param>
        /// <param name="status">new status</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task UpdateBookingAsync(Guid id, BookingStatus status, CancellationToken token);
    }
}
