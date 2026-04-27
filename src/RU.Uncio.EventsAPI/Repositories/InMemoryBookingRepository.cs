using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using System.Collections.Concurrent;

namespace RU.Uncio.EventsAPI.Repositories
{
    public class InMemoryBookingRepository : IBookingRepository
    {
        public readonly ConcurrentDictionary<Guid, Booking> Bookings = new();

        public async Task<bool> AddBookingAsync(Booking book)
        {
            return Bookings.TryAdd(book.Id, book);
        }

        public async Task<Dictionary<Guid, Booking>> GetBookingsAsync()
        {
            return Bookings.ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task UpdateBookingAsync(Guid id, BookingStatus status)
        {
            Bookings[id].Status = status;
        }
    }
}
