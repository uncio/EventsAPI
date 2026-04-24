using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Repositories
{
    public class InMemoryBookingRepository : IBookingRepository
    {
        public static Dictionary<Guid, Booking> Bookings { get; private set; } = [];

        public void AddBooking(Booking book)
        {
            Bookings.Add(book.Id, book);
        }

        public Dictionary<Guid, Booking> GetBookings()
        {
            return Bookings;
        }

        public void RemoveBooking(Guid id)
        {
            Bookings.Remove(id);
        }

        public void UpdateBooking(Guid id, BookingStatus status)
        {
            Bookings[id].Status = status;
        }
    }
}
