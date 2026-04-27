using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
{
    /// <summary>
    /// Service to manipulate with bookings collection and background queue 
    /// </summary>
    public class BookingService : BackgroundService, IBookingService
    {
        private readonly ILogger<BookingService> logger;
        private readonly IBookingRepository repository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="bookingRepo"></param>
        public BookingService(ILogger<BookingService> log, IBookingRepository bookingRepo)
        {
            logger = log;
            repository = bookingRepo;
        }

        /// <summary>
        /// Creates a booking asynchronously
        /// </summary>
        /// <param name="eventId">event id of the new booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken token)
        {
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

        /// <summary>
        /// Background service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var bookings = await repository.GetBookingsAsync(stoppingToken);

                    var pendingBooking = bookings.Values
                        ?.FirstOrDefault(b => b.Status == BookingStatus.Pending);
                    if (pendingBooking != null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        await repository.UpdateBookingAsync(pendingBooking.Id, BookingStatus.Confirmed, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Booking manipulation error");
                }
            }
        }
    }
}
