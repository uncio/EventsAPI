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
        private readonly AppDbContext appDbContext;
        //private readonly IBookingRepository repository;

        private readonly object bookingLock = new();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="bookingRepo"></param>
        /// <param name="evService"></param>
        public BookingService(ILogger<BookingService> log, AppDbContext context,/*IBookingRepository bookingRepo,*/ IEventsService evService)
        {
            logger = log;
            //repository = bookingRepo;
            eventService = evService;
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
            var ev = await eventService.GetEventAsync(eventId);
            if (ev == null)
            {
                logger.LogError($"Event with ID {eventId} is not found in the collection");
                throw new MissingEventException($"Event with ID {eventId} is not found in the collection");
            }

            var bookingResult = false;

            lock (bookingLock)
            {
                bookingResult = ev.TryReserveSeats();
            }

            if (!bookingResult)
            {
                throw new NoAvailableSeatsException("No available seats for this event");
            }

            var newBooking = new Booking(eventId);

            await appDbContext.Bookings.AddAsync(newBooking);           
            await appDbContext.SaveChangesAsync();

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
            //var bookings = await repository.GetBookingsAsync(token);
            var bookings = appDbContext.Bookings
                .ToDictionary(b => b.Id);

            if (bookings.TryGetValue(bookingId, out var booking))
                return booking;

            logger.LogError($"Booking queue doesn't contain a booking with id {bookingId}");
            return null;
        }        
    }
}
