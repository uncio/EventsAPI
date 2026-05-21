using RU.Uncio.EventsAPI.Models;
using System.Collections.Concurrent;

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
        /// Gets all pending bookings
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ConcurrentBag<Booking>> GetPendingBookingsAsync(CancellationToken token);
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
        /// <param name="booking">updating booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task UpdateBookingAsync(Booking booking, CancellationToken token);
    }
}
