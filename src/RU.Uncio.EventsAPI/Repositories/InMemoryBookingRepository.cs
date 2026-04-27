using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using System.Collections.Concurrent;

namespace RU.Uncio.EventsAPI.Repositories
{
    /// <summary>
    /// Concrete in memory bookings repository
    /// </summary>
    public class InMemoryBookingRepository : IBookingRepository
    {
        /// <summary>
        /// Collection of bookings
        /// </summary>
        public readonly ConcurrentDictionary<Guid, Booking> Bookings = new();

        /// <summary>
        /// Adds a booking to collection
        /// </summary>
        /// <param name="book">booking to add</param>
        /// <param name="token"></param>
        /// <returns>result of adding, true if succeded</returns>
        public async Task<bool> AddBookingAsync(Booking book, CancellationToken token)
        {
            return Bookings.TryAdd(book.Id, book);
        }

        /// <summary>
        /// Gets all bookings from collection
        /// </summary>
        /// <param name="token"></param>
        /// <returns>collection of existing bookings</returns>
        public async Task<Dictionary<Guid, Booking>> GetBookingsAsync(CancellationToken token)
        {
            return Bookings.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Updates a booking status in collection by booking ID
        /// </summary>
        /// <param name="id">id of booking to update</param>
        /// <param name="status">new status</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UpdateBookingAsync(Guid id, BookingStatus status, CancellationToken token)
        {
            Bookings[id].Status = status;
        }
    }
}
