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
        private readonly IServiceScopeFactory scopeFactory;
        private readonly SemaphoreSlim processingSemaphore = new(1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scFactory"></param>
        /// <param name="log"></param>
        public BookingBackgroundService(IServiceScopeFactory scFactory, ILogger<BookingBackgroundService> log)
        {
            scopeFactory = scFactory;
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
                    using var scope = scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                    var pendingBookings = await repository.GetPendingBookingsAsync(stoppingToken);

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

            await processingSemaphore.WaitAsync();

            try
            {
                using var scope = scopeFactory.CreateScope();
                var bookRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

                if (eventRepository.GetEvents().TryGetValue(booking.EventId, out Event ev))
                {
                    try
                    {
                        await bookRepository.UpdateBookingAsync(booking.Id, BookingStatus.Confirmed, stoppingToken);
                    }
                    catch(Exception ex)
                    {
                        await bookRepository.UpdateBookingAsync(booking.Id, BookingStatus.Rejected, stoppingToken);
                        ev.ReleaseSeats();
                        logger.LogWarning($"Failed to book an event with ID {booking.EventId}");
                        throw;
                    }                    
                }
                else
                {
                    await bookRepository.UpdateBookingAsync(booking.Id, BookingStatus.Rejected, stoppingToken);
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
