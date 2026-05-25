using Microsoft.EntityFrameworkCore;
using RU.Uncio.EventsAPI.DataAccess;
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
        private static readonly SemaphoreSlim processingSemaphore = new(1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookings"></param>
        /// <param name="events"></param>
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
                    var repository = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var pendingBookings = await repository.Bookings
                        .Where(b => b.Status == BookingStatus.Pending).ToListAsync();

                    scope.Dispose();

                    if (pendingBookings != null && pendingBookings.Any())
                    {
                        var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking.Id, stoppingToken));
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

        private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await processingSemaphore.WaitAsync(stoppingToken);

            try
            {
                var targetBooking = repository.Bookings.FirstOrDefault(b => b.Id == bookingId);
                if(targetBooking == null)
                {
                    throw new ArgumentException($"Booking with id {bookingId} doesn't exist");
                }

                var existingEvent = repository.Events.FirstOrDefault(ev => targetBooking.EventId == ev.Id);
                if (existingEvent != null)
                {
                    try
                    {
                        targetBooking.Confirm();
                    }
                    catch(Exception ex)
                    {
                        targetBooking.Reject();                        
                        existingEvent.ReleaseSeats();
                        logger.LogError(ex, $"Failed to book an event with ID {targetBooking.EventId}");
                    }                    
                }
                else
                {
                    targetBooking.Reject();
                    logger.LogWarning($"Failed to book an event with ID {targetBooking.EventId}");
                }

                repository.Update(targetBooking);
                await repository.SaveChangesAsync();
            }
            finally
            {
                processingSemaphore.Release();
            }
        }
    }
}
