using Microsoft.EntityFrameworkCore;
using RU.Uncio.EventsAPI.DataAccess;
using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using System.Net;

namespace RU.Uncio.EventsAPI.Services
{
    /// <summary>
    /// Service to manipulate with bookings collection and background queue 
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly AppDbContext appDbContext;

        private static readonly SemaphoreSlim bookingSemaphore = new(1, 1);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="context"></param>
        public BookingService(AppDbContext context)
        {
            appDbContext = context;
        }

        /// <summary>
        /// Creates a booking asynchronously
        /// </summary>
        /// <param name="eventId">event id of the new booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken token)
        {
            await bookingSemaphore.WaitAsync(token);
            Booking? newBooking = null;
            try
            {
                var ev = await appDbContext.Events.FirstOrDefaultAsync(ev => ev.Id == eventId);
                if (ev == null)
                {
                    throw new MissingEventException($"Event with ID {eventId} is not found in the collection");
                }

                var bookingResult = ev.TryReserveSeats();

                if (!bookingResult)
                {
                    throw new NoAvailableSeatsException("No available seats for this event");
                }

                newBooking = new Booking(eventId);

                await appDbContext.Bookings.AddAsync(newBooking);
                await appDbContext.SaveChangesAsync();
            }
            finally
            {
                bookingSemaphore.Release();
            }            

            var added = appDbContext.Bookings.FirstOrDefault(b => b.Id == newBooking.Id);

            return added != null ? newBooking : null;
        }

        /// <summary>
        /// Gets a booking asynchronously by booking ID
        /// </summary>
        /// <param name="bookingId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken token)
        {
            var bookings = appDbContext.Bookings
                .ToDictionary(b => b.Id);

            if (bookings.TryGetValue(bookingId, out var booking))
                return booking;

            return null;
        }        
    }
}
