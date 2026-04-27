using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using System.Collections.Concurrent;

namespace RU.Uncio.EventsAPI.Repositories
{
    public class InMemoryBookingRepository : IBookingRepository
    {
        public static ConcurrentDictionary<Guid, Booking> Bookings { get; private set; } = [];

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
