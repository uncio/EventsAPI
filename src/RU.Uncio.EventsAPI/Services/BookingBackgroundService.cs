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

                    var bookings = await repository.GetBookingsAsync(stoppingToken);

                    var pendingBooking = bookings.Values
                        ?.FirstOrDefault(b => b.Status == BookingStatus.Pending);
                    if (pendingBooking != null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        await repository.UpdateBookingAsync(pendingBooking.Id, BookingStatus.Confirmed, stoppingToken);
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
    }
}
