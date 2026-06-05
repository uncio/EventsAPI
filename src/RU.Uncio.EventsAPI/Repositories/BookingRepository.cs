using Microsoft.EntityFrameworkCore;
using RU.Uncio.EventsAPI.DataAccess;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using System.Collections.Concurrent;

namespace RU.Uncio.EventsAPI.Repositories
{
    /// <summary>
    /// Concrete in memory bookings repository
    /// </summary>
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext db;
        public BookingRepository(AppDbContext dB) { db = dB; }

        /// <summary>
        /// Adds a booking to collection
        /// </summary>
        /// <param name="book">booking to add</param>
        /// <param name="token"></param>
        /// <returns>result of adding, true if succeded</returns>
        public async Task<bool> AddBookingAsync(Booking book, CancellationToken token)
        {
            await db.Bookings.AddAsync(book, token); 
            await db.SaveChangesAsync(token);

            return true;
        }

        /// <summary>
        /// Gets all bookings from collection
        /// </summary>
        /// <param name="token"></param>
        /// <returns>collection of existing bookings</returns>
        public async Task<Dictionary<Guid, Booking>> GetBookingsAsync(CancellationToken token)
        {
            return await db.Bookings.ToDictionaryAsync(x => x.Id, token);
        }

        /// <summary>
        /// Gets all pending bookings
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ConcurrentBag<Booking>> GetPendingBookingsAsync(CancellationToken token)
        {
            var result = db.Bookings
                .Where(b => b.Status == BookingStatus.Pending);
            return [.. result];
        }

        /// <summary>
        /// Updates a booking status in collection by booking ID
        /// </summary>
        /// <param name="booking">updating booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UpdateBookingAsync(Booking booking, CancellationToken token)
        {
            db.Bookings.Update(booking);
            await db.SaveChangesAsync();
        }
    }
}
