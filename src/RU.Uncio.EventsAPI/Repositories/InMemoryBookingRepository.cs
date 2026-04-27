using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using System.Collections.Concurrent;

namespace RU.Uncio.EventsAPI.Repositories
{
    public class InMemoryBookingRepository : IBookingRepository
    {
        public readonly ConcurrentDictionary<Guid, Booking> Bookings = new();

        public bool AddBooking(Booking book)
        {
            return Bookings.TryAdd(book.Id, book);
        }

        public Dictionary<Guid, Booking> GetBookings()
        {
            return Bookings.ToDictionary(x => x.Key, x => x.Value);
        }

        public void UpdateBooking(Guid id, BookingStatus status)
        {
            Bookings[id].Status = status;
        }
    }
}
