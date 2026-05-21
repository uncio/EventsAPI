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
        public static ConcurrentDictionary<Guid, Booking> Bookings = new();

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
        /// Gets all pending bookings
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ConcurrentBag<Booking>> GetPendingBookingsAsync(CancellationToken token)
        {
            return new ConcurrentBag<Booking>(Bookings
                .Where(b => b.Value.Status == BookingStatus.Pending)
                .Select(b => b.Value));
        }

        /// <summary>
        /// Updates a booking status in collection by booking ID
        /// </summary>
        /// <param name="booking">updating booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UpdateBookingAsync(Booking booking, CancellationToken token)
        {
            Bookings[booking.Id].Status = booking.Status;
            Bookings[booking.Id].ProcessedAt = booking.ProcessedAt;
        }
    }
}
