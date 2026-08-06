
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RU.Uncio.BookingService.Application.Interfaces;
using RU.Uncio.BookingService.Domain.Exceptions;
using RU.Uncio.BookingService.Domain.Models;

namespace RU.Uncio.BookingService.Application.Services
{
    /// <summary>
    /// Service to manipulate with bookings collection and background queue 
    /// </summary>
    public class BookService : IBookingService
    {
        private readonly ILogger<BookService> logger;
        //private readonly IUserService usersService;
        //private readonly IEventsService eventService;
        private readonly IBookingRepository repository;

        private static readonly SemaphoreSlim bookingSemaphore = new(1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="bookingRepo"></param>
        public BookService(ILogger<BookService> log, IBookingRepository bookingRepo)
        {
            logger = log;
            repository = bookingRepo;
        }

        /// <summary>
        /// Creates a booking asynchronously
        /// </summary>
        /// <param name="userId">user id for the new booking</param>
        /// <param name="eventId">event id of the new booking</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Booking> CreateBookingAsync(Guid userId, Guid eventId, CancellationToken token)
        {
            var added = false;
            await bookingSemaphore.WaitAsync(token);
            Booking? newBooking = null;
            try
            {
                newBooking = new Booking(userId, eventId);
                added = await repository.AddBookingAsync(newBooking, token);
            }
            finally
            {
                bookingSemaphore.Release();
            }            

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
        /// Cancels booking for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="bookingId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="MissingUserException"></exception>
        /// <exception cref="NoRightsException"></exception>
        /// <exception cref="MissingEventException"></exception>
        /// <exception cref="MissingBookingException"></exception>
        public async Task CancelBookingByIdAsync(Guid userId, string userRole, Guid bookingId, CancellationToken token)
        {
            await bookingSemaphore.WaitAsync(token);

            var bookings = await repository.GetBookingsAsync(token);

            try
            {
                if (bookings.TryGetValue(bookingId, out var booking))
                {
                    if (!booking.UserId.Equals(userId) && !userRole.Equals("Admin"))
                    {
                        throw new NoRightsException($"User with ID {userId} can't cancell the booking with ID {bookingId}");
                    }

                    booking.Cancell();
                    await repository.UpdateBookingAsync(booking, token);
                }
                else
                {
                    throw new MissingBookingException($"Booking with ID {booking} is not found in DB");
                }
            }
            finally
            {
                bookingSemaphore.Release();
            }
        }
    }
}
