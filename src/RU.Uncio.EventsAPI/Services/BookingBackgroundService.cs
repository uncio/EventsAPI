using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
{
    /// <summary>
    /// Background watcher for new bookings
    /// </summary>
    public class BookingBackgroundService: BackgroundService
    {
        private readonly ILogger<BookingBackgroundService> logger;
        private readonly IEventRepository eventRepository;
        private readonly IBookingRepository bookingRepository;
        private readonly SemaphoreSlim processingSemaphore = new(1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookings"></param>
        /// <param name="events"></param>
        /// <param name="log"></param>
        public BookingBackgroundService(IBookingRepository bookings, IEventRepository events, ILogger<BookingBackgroundService> log)
        {
            bookingRepository = bookings;
            eventRepository = events;
            logger = log;
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
                    var pendingBookings = await bookingRepository.GetPendingBookingsAsync(stoppingToken);

                    if (pendingBookings != null && pendingBookings.Any())
                    {
                        var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
                        await Task.WhenAll(tasks);                        
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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

        private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            await processingSemaphore.WaitAsync(stoppingToken);

            try
            {
                if (eventRepository.GetEvents().TryGetValue(booking.EventId, out var ev))
                {
                    try
                    {
                        booking.Confirm();
                        await bookingRepository.UpdateBookingAsync(booking, stoppingToken);
                    }
                    catch(Exception ex)
                    {
                        booking.Reject();
                        await bookingRepository.UpdateBookingAsync(booking, stoppingToken);
                        ev.ReleaseSeats();
                        logger.LogError(ex, $"Failed to book an event with ID {booking.EventId}");
                    }                    
                }
                else
                {
                    booking.Reject();
                    await bookingRepository.UpdateBookingAsync(booking, stoppingToken);
                    logger.LogWarning($"Failed to book an event with ID {booking.EventId}");
                }
            }
            finally
            {
                processingSemaphore.Release();
            }
        }
    }
}
