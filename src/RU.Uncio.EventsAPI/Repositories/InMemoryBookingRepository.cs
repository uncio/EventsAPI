using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Repositories
{
    public class InMemoryBookingRepository : IBookingRepository
    {
        public static Dictionary<Guid, Booking> Bookings { get; private set; } = [];

        public async Task AddBookingAsync(Booking book)
        {
            Bookings.Add(book.Id, book);
        }

        public async Task<Dictionary<Guid, Booking>> GetBookingsAsync()
        {
            return Bookings;
        }

        public async Task RemoveBookingAsync(Guid id)
        {
            Bookings.Remove(id);
        }

        public async Task UpdateEventAsync(Guid id, BookingStatus status)
        {
            Bookings[id].Status = status;
        }
    }
}
