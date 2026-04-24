using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
{
    public class BookingService : BackgroundService, IBookingService
    {
        private readonly ILogger<BookingService> logger;
        private readonly IServiceScopeFactory scopeFactory;

        public BookingService(ILogger<BookingService> log, IServiceScopeFactory scope)
        {
            logger = log;
            scopeFactory = scope;
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            using var scope = scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider
                .GetRequiredService<IBookingRepository>();
            var newBooking = new Booking(eventId);

            bookingService.AddBooking(newBooking);

            return newBooking;
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            using var scope = scopeFactory.CreateScope();
            var bookingRepo = scope.ServiceProvider
                .GetRequiredService<IBookingRepository>();

            var bookings = bookingRepo.GetBookings();

            if (bookings.TryGetValue(bookingId, out var booking))
                return booking;
            return null;
            //throw new BookingNotFoundException($"Booking queue doesn't contain a booking with id {bookingId}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    //try
            //    //{
            //    //    if (_taskQueue.TryDequeue(out var task))
            //    //    {
            //    //        _logger.LogInformation(
            //    //            "Начата генерация отчёта {TaskId}, тип: {ReportType}",
            //    //            task.Id, task.ReportType);

            //    //        // Имитация долгой генерации отчёта
            //    //        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            //    //        _logger.LogInformation(
            //    //            "Отчёт {TaskId} сгенерирован успешно", task.Id);
            //    //    }
            //    //}
            //    //catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            //    //{
            //    //    break;
            //    //}
            //    //catch (Exception ex)
            //    //{
            //    //    _logger.LogError(ex, "Ошибка при генерации отчёта");
            //    //}

            //    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            //}
        }
    }
}
