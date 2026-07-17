
using Microsoft.Extensions.Logging;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.Domain.Exceptions;
using RU.Uncio.Domain.Models;

namespace RU.Uncio.Application.Services
{
    /// <summary>
    /// Service to manipulate with bookings collection and background queue 
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly ILogger<BookingService> logger;
        private readonly IUserService usersService;
        private readonly IEventsService eventService;
        private readonly IBookingRepository repository;

        private static readonly SemaphoreSlim bookingSemaphore = new(1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="bookingRepo"></param>
        /// <param name="evService"></param>
        /// <param name="usService"></param>
        public BookingService(ILogger<BookingService> log, IBookingRepository bookingRepo, IEventsService evService, IUserService usService)
        {
            logger = log;
            repository = bookingRepo;
            eventService = evService;
            usersService = usService;
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
                var user = await usersService.GetUserByIdAsync(userId, token);
                if (user == null)
                {
                    throw new MissingUserException($"User with ID {userId} is not found in DB");
                }

                var ev = await eventService.GetEventAsync(eventId, token);
                if (ev == null)
                {
                    throw new MissingEventException($"Event with ID {eventId} is not found in the collection");
                }

                if (DateTime.Now.IsStrictlyGreaterThan(ev.StartAt))
                {
                    throw new EventExpiredException($"Event with ID {eventId} has been already started");
                }

                newBooking = new Booking(userId, eventId);

                if (!user.TryAddBooking(newBooking))
                {
                    throw new ExceededBookingLimitException($"User with ID {userId} already booked 10 events");
                }

                var bookingResult = ev.TryReserveSeats();

                if (!bookingResult)
                {
                    throw new NoAvailableSeatsException("No available seats for this event");
                }

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
        public async Task CancelBookingByIdAsync(Guid userId, Guid bookingId, CancellationToken token)
        {
            await bookingSemaphore.WaitAsync(token);

            var bookings = await repository.GetBookingsAsync(token);

            try
            {
                if (bookings.TryGetValue(bookingId, out var booking))
                {
                    var user = await usersService.GetUserByIdAsync(userId, token);
                    if (user == null)
                    {
                        throw new MissingUserException($"User with ID {userId} is not found in DB");
                    }

                    if (!booking.UserId.Equals(userId) && user.Role != Roles.Admin)
                    {
                        throw new NoRightsException($"User with ID {userId} can't cancell the booking with ID {bookingId}");
                    }

                    var ev = await eventService.GetEventAsync(booking.EventId, token);
                    if (ev == null)
                    {
                        throw new MissingEventException($"Event with ID {booking.EventId} is not found in DB");
                    }

                    booking.Cancell();
                    user.RemoveBooking(bookingId);
                    ev.ReleaseSeats();
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
