using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
{
    public class BookingService : BackgroundService, IBookingService
    {
        private readonly ILogger<BookingService> logger;
        private readonly IBookingRepository repository;

        public BookingService(ILogger<BookingService> log, IBookingRepository bookingRepo)
        {
            logger = log;
            repository = bookingRepo;
        }

        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            var newBooking = new Booking(eventId);

            var added = repository.AddBooking(newBooking);

            return added ? newBooking : null;
        }

        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            var bookings = repository.GetBookings();

            if (bookings.TryGetValue(bookingId, out var booking))
                return booking;
            return null;
            //throw new BookingNotFoundException($"Booking queue doesn't contain a booking with id {bookingId}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var pendingBooking = repository.GetBookings().Values
                        ?.FirstOrDefault(b => b.Status == BookingStatus.Pending);
                    if (pendingBooking != null)
                    {
                        //_logger.LogInformation(
                        //    "Начата генерация отчёта {TaskId}, тип: {ReportType}",
                        //    task.Id, task.ReportType);

                        // Имитация долгой генерации отчёта
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                        //_logger.LogInformation(
                        //    "Отчёт {TaskId} сгенерирован успешно", task.Id);

                        repository.UpdateBooking(pendingBooking.Id, BookingStatus.Confirmed);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "Ошибка при генерации отчёта");
                }
            }
        }
    }
}
