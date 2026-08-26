using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RU.Uncio.BookingService.Application.Interfaces;

namespace RU.Uncio.BookingService.Application.Backservices
{
    /// <summary>
    /// Background watcher for new bookings
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="scFactory"></param>
    /// <param name="log"></param>
    public class BookingBackgroundService(IServiceScopeFactory scFactory,
                                          ILogger<BookingBackgroundService> log) : BackgroundService
    {
        private readonly ILogger<BookingBackgroundService> logger = log;
        private readonly IServiceScopeFactory scopeFactory = scFactory;
        private static readonly SemaphoreSlim processingSemaphore = new(1, 1);

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

                    scope.Dispose();

                    if (pendingBookings != null && !pendingBookings.IsEmpty)
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
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var producer = scope.ServiceProvider.GetRequiredService<IBookingProducer>();
            //var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            await processingSemaphore.WaitAsync(stoppingToken);

            try
            {
                var bookings = await bookingRepository.GetBookingsAsync(stoppingToken);

                if(bookings.TryGetValue(bookingId, out var targetBooking))
                {
                    try
                    {
                        targetBooking.Confirm();
                    }
                    catch (Exception ex)
                    {
                        targetBooking.Reject();
                        logger.LogError(ex, "Failed to book an event with ID {BookingId}", targetBooking.EventId);
                    }

                    await bookingRepository.UpdateBookingAsync(targetBooking, stoppingToken);

                    if(targetBooking.Status == Domain.Models.BookingStatus.Confirmed)
                    {
                        await producer.PublishBooking(targetBooking.Id, targetBooking.EventId, targetBooking.UserId, 1);
                    }
                }
                else
                {
                    throw new ArgumentException($"Booking with id {bookingId} doesn't exist");
                }
            }
            finally
            {
                processingSemaphore.Release();
            }
        }
    }
}
