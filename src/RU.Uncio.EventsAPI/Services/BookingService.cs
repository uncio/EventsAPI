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
        private readonly ILogger<BookingService> logger;
        private readonly IEventsService eventService;
        private readonly IBookingRepository repository;

        private static readonly SemaphoreSlim bookingSemaphore = new(1, 1);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="context"></param>
        public BookingService(ILogger<BookingService> log, IBookingRepository bookingRepo, IEventsService evService)
        {
            logger = log;
            repository = bookingRepo;
            eventService = evService;
        }

        /// <summary>
        /// Creates a booking asynchronously
        /// </summary>
        /// <param name="eventId">event id of the new booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken token)
        {
            var added = false;
            await bookingSemaphore.WaitAsync(token);
            Booking? newBooking = null;
            try
            {
                var ev = await eventService.GetEventAsync(eventId, token);
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

                added = await repository.AddBookingAsync(newBooking, token);
            }
            finally
            {
                bookingSemaphore.Release();
            }            

            return added ? newBooking : null;
        }

        /// <summary>
        /// Gets a booking asynchronously by booking ID
        /// </summary>
        /// <param name="bookingId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken token)
        {
            var bookings = await repository.GetBookingsAsync(token);

            if (bookings.TryGetValue(bookingId, out var booking))
                return booking;

            logger.LogError($"Booking queue doesn't contain a booking with id {bookingId}");
            return null;
        }        
    }
}
