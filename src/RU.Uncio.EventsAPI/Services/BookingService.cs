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

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="bookingRepo"></param>
        /// <param name="evService"></param>
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
            var ev = eventService.GetEvent(eventId);
            if (ev == null)
            {
                logger.LogError($"Event with ID {eventId} is not found in the collection");
                throw new MissingEventException($"Event with ID {eventId} is not found in the collection");
            }

            var newBooking = new Booking(eventId);
            var added = await repository.AddBookingAsync(newBooking, token);

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
