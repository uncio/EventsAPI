using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.Application.Services;
using RU.Uncio.Domain.Exceptions;
using RU.Uncio.Domain.Models;

namespace EventsAPI.Tests
{
    public class BookingServiceTests
    {
        private readonly Mock<ILogger<BookingService>> bookingsLogger;
        private readonly EventsService eventsService;
        private readonly UserService userService;
        private readonly Dictionary<Guid, Event> events;
        private readonly List<User> users;

        public BookingServiceTests()
        {
            bookingsLogger = new Mock<ILogger<BookingService>>();
            var mockRepository = new Mock<IEventRepository>();
            var logger = new Mock<ILogger<EventsService>>();
            eventsService = new EventsService(logger.Object, mockRepository.Object);
            events = new List<Event>
                {
                    new("Event1", new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 2),
                    new("Event2",new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 2),
                }
                .ToDictionary(ev => ev.Id, events => events);

            mockRepository.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(events);

            var authLogger = new Mock<ILogger<AuthService>>();
            var config = new Mock<IConfiguration>();
            var authService = new AuthService(config.Object);
            var mockUserRepository = new Mock<IUserRepository>();
            var uLogger = new Mock<ILogger<UserService>>();
            userService = new UserService(uLogger.Object, mockUserRepository.Object, authService);
            users = new List<User>
                {
                    new("User1234","12345678"),
                    new("Admin123", "admin123", Roles.Admin)
                };

            mockUserRepository.Setup(method => method.GetAllUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);
            mockUserRepository.Setup(method => method.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken t) => users.FirstOrDefault(u => u.Id == id)!);
        }

        [Fact]
        public async Task AddBookingForExistingEvent_ReturnsPendingBooking()
        {
            //Arrange
            var eventToBook = events.FirstOrDefault().Value;
            var user = users.FirstOrDefault()!;
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking = await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(newBooking);
            Assert.Equal(BookingStatus.Pending, newBooking.Status);
            Assert.Equal(eventToBook.Id, newBooking.EventId);
            Assert.Equal(user.Id, newBooking.UserId);
        }

        [Fact]
        public async Task AddBookingForExistingEvent_DecreasesAvailableSeats()
        {
            //Arrange
            var eventToBook = events.FirstOrDefault().Value;
            var user = users.FirstOrDefault()!;
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var currentAvailableSeats = eventToBook.AvailableSeats;
            var newBooking = await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken);
            var updatedAvailableSeats = eventToBook.AvailableSeats;

            // Assert
            Assert.Equal(currentAvailableSeats - 1, updatedAvailableSeats);
        }

        [Fact]
        public async Task AddSeveralBookingsForSameExistingEvent_ReturnsBookingsWithDifferentIdsNotEmpty()
        {
            //Arrange
            var eventToBook1 = events.FirstOrDefault().Value;
            var user = users.FirstOrDefault()!;

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking1 = await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook1.Id, TestContext.Current.CancellationToken);
            var newBooking2 = await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook1.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotEqual(new Guid(), newBooking1.Id);
            Assert.NotEqual(new Guid(), newBooking2.Id);
            Assert.NotEqual(newBooking1.Id, newBooking2.Id);
        }

        [Fact]
        public async Task GetBookingById_ReturnsExpectingBooking()
        {
            //Arrange
            var eventToBook = events.FirstOrDefault().Value;
            var user = users.FirstOrDefault()!;

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService, userService);
            var initialBookings = new Dictionary<Guid, Booking>();


            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            // Act
            var newBooking = await bookingService.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken);
            var result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newBooking.Id, result.Id);
            Assert.Equal(eventToBook.Id, result.EventId);
            Assert.Equal(user.Id, result.UserId);
        }

        [Fact]
        public async Task UpdateBookingStatus_UpdatesExpectingBooking()
        {
            //Arrange
            var eventToBook = events.FirstOrDefault().Value;
            var user = users.FirstOrDefault()!;

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            bookingRepoMock.Setup(method => method.UpdateBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((booking, token) => booking.Confirm());

            // Act
            var newBooking = await bookingService.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken);
            var result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            var resultBefore = result.Status;

            newBooking.Confirm();
            await bookingRepoMock.Object.UpdateBookingAsync(newBooking, TestContext.Current.CancellationToken);
            result = await bookingService.GetBookingByIdAsync(newBooking.Id, TestContext.Current.CancellationToken);

            var resultAfter = result.Status;

            // Assert
            Assert.Equal(BookingStatus.Pending, resultBefore);
            Assert.NotEqual(BookingStatus.Pending, resultAfter);
        }



        [Fact]
        public async Task GetBookingById_WhenIdDoesntExist_ReturnsNull()
        {
            //Arrange
            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);

            var id = Guid.NewGuid();

            // Act
            var result = await bookingService.GetBookingByIdAsync(id, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddEventBooking_WhenEventDoesntExist_ThrowsMissingEvent()
        {
            //Arrange
            var eventToBook = new Event("Test", DateTime.Now, DateTime.Now + TimeSpan.FromHours(4), 10);
            var user = users.FirstOrDefault()!;
            var expectedExceptionMessage = $"Event with ID {eventToBook.Id} is not found in the collection";

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsService, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = await Assert
                .ThrowsAsync<MissingEventException>(async () => await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddEventBooking_WhenEventRemoved_ThrowsMissingEvent()
        {
            //Arrange
            var mockRepositoryLocal = new Mock<IEventRepository>();
            var loggerLocal = new Mock<ILogger<EventsService>>();
            var eventsServiceLocal = new EventsService(loggerLocal.Object, mockRepositoryLocal.Object);
            var eventsLocal = new List<Event>
                {
                    new("Event1", new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 10),
                    new("Event2",new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 10),
                    new("Event22",new DateTime(2027, 1, 15), new DateTime(2027, 1, 16), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepositoryLocal.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventsLocal);
            mockRepositoryLocal.Setup(method => method.RemoveEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, CancellationToken>((guid, token) => eventsLocal.Remove(guid));

            var eventToRemove = eventsLocal.LastOrDefault().Value;
            var user = users.FirstOrDefault()!;
            await eventsServiceLocal.RemoveEventAsync(eventToRemove.Id, TestContext.Current.CancellationToken);

            var expectedExceptionMessage = $"Event with ID {eventToRemove.Id} is not found in the collection";

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsServiceLocal, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = await Assert
                .ThrowsAsync<MissingEventException>(async () => await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToRemove.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddBookingsForSameExistingEvent_WhenOverAvailableSeats_ThrowsNoAvailableSeats()
        {
            //Arrange
            var mockRepositoryLocal = new Mock<IEventRepository>();
            var loggerLocal = new Mock<ILogger<EventsService>>();
            var eventsServiceLocal = new EventsService(loggerLocal.Object, mockRepositoryLocal.Object);
            var eventsLocal = new List<Event>
                {
                    new("Event1", new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 1)
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepositoryLocal.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventsLocal);
            mockRepositoryLocal.Setup(method => method.RemoveEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, CancellationToken>((guid, token) => eventsLocal.Remove(guid));

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsServiceLocal, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            var expectedExceptionMessage = $"No available seats for this event";

            bookingRepoMock.Setup(method => method.GetBookingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialBookings);
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            var user = users.FirstOrDefault()!;

            // Act
            var newBooking1 = await bookingServiceToAdd.CreateBookingAsync(user.Id, eventsLocal.FirstOrDefault().Key, TestContext.Current.CancellationToken);

            var exception = await Assert
                .ThrowsAsync<NoAvailableSeatsException>(() =>
                bookingServiceToAdd.CreateBookingAsync(user.Id, eventsLocal.FirstOrDefault().Key, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<NoAvailableSeatsException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task ConfirmBooking_ReturnsConfirmedStatusAndFilledProcessedAtAndDecreasesAvailableSeats()
        {
            //Arrange
            var mockRepositoryLocal = new Mock<IEventRepository>();
            var loggerLocal = new Mock<ILogger<EventsService>>();
            var eventsServiceLocal = new EventsService(loggerLocal.Object, mockRepositoryLocal.Object);
            var eventsLocal = new List<Event>
                {
                    new("Event1", new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 10)
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepositoryLocal.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventsLocal);
            mockRepositoryLocal.Setup(method => method.RemoveEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, CancellationToken>((guid, token) => eventsLocal.Remove(guid));

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsServiceLocal, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            var eventToBook = eventsLocal.FirstOrDefault().Value;
            var user = users.FirstOrDefault()!;

            // Act
            var seatsBefore = eventToBook.AvailableSeats;
            var newBooking = await bookingService.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken);
            var timeBefore = newBooking.ProcessedAt;
            newBooking.Confirm();
            var timeAfter = newBooking.ProcessedAt;
            var seatsAfter = eventToBook.AvailableSeats;

            // Assert
            Assert.Equal(BookingStatus.Confirmed, newBooking.Status);
            Assert.Null(timeBefore);
            Assert.NotNull(timeAfter);
            Assert.Equal(seatsBefore, seatsAfter + 1);
        }

        [Fact]
        public async Task RejectBooking_ReturnsRejectedStatusAndFilledProcessedAtAndReleasesSeats()
        {
            //Arrange
            var mockRepositoryLocal = new Mock<IEventRepository>();
            var loggerLocal = new Mock<ILogger<EventsService>>();
            var eventsServiceLocal = new EventsService(loggerLocal.Object, mockRepositoryLocal.Object);
            var eventsLocal = new List<Event>
                {
                    new("Event1", new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 1)
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepositoryLocal.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventsLocal);
            mockRepositoryLocal.Setup(method => method.RemoveEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Callback<Guid, CancellationToken>((guid, token) => eventsLocal.Remove(guid));

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingService = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsServiceLocal, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);
            var eventToBook = eventsLocal.FirstOrDefault().Value;
            var user = users.FirstOrDefault()!;

            // Act
            var seatsBefore = eventToBook.AvailableSeats;
            var newBooking = await bookingService.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken);
            var timeBefore = newBooking.ProcessedAt;

            newBooking.Reject();
            eventToBook.ReleaseSeats();

            var timeAfter = newBooking.ProcessedAt;
            var seatsAfter = eventToBook.AvailableSeats;

            // Assert
            Assert.Equal(BookingStatus.Rejected, newBooking.Status);
            Assert.Null(timeBefore);
            Assert.NotNull(timeAfter);
            Assert.Equal(seatsBefore, seatsAfter);
        }


        [Fact]
        public async Task AddEventBooking_WhenEventExpired_ThrowsExpiredEvent()
        {
            //Arrange
            var mockRepositoryLocal = new Mock<IEventRepository>();
            var loggerLocal = new Mock<ILogger<EventsService>>();
            var eventsServiceLocal = new EventsService(loggerLocal.Object, mockRepositoryLocal.Object);
            var eventsLocal = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepositoryLocal.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventsLocal);

            var eventToBook = eventsLocal.FirstOrDefault().Value;

            var user = users.FirstOrDefault()!;
            var expectedExceptionMessage = $"Event with ID {eventToBook.Id} has been already started";

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsServiceLocal, userService);
            var initialBookings = new Dictionary<Guid, Booking>();

            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = await Assert
                .ThrowsAsync<EventExpiredException>(async () => await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<EventExpiredException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddEventBooking_WhenUserAchivedBookingLimit_ThrowsExceededBooking()
        {
            //Arrange
            var mockRepositoryLocal = new Mock<IEventRepository>();
            var loggerLocal = new Mock<ILogger<EventsService>>();
            var eventsServiceLocal = new EventsService(loggerLocal.Object, mockRepositoryLocal.Object);
            var eventsLocal = new List<Event>
                {
                    new("Event1", new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepositoryLocal.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventsLocal);

            var eventToBook = eventsLocal.FirstOrDefault().Value;

            var authLogger = new Mock<ILogger<AuthService>>();
            var config = new Mock<IConfiguration>();
            var authService = new AuthService(config.Object);
            var mockUserRepository = new Mock<IUserRepository>();
            var uLogger = new Mock<ILogger<UserService>>();
            var localUserService = new UserService(uLogger.Object, mockUserRepository.Object, authService);
            var localUsers = new List<User>
                {
                    new("User1234","12345678")
                };

            //mockUserRepository.Setup(method => method.GetAllUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(localUsers);
            mockUserRepository.Setup(method => method.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken t) => localUsers.FirstOrDefault(u => u.Id == id)!);

            var user = localUsers.FirstOrDefault()!;
            var expectedExceptionMessage = $"User with ID {user.Id} already booked 10 events";

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsServiceLocal, localUserService);
            var initialBookings = new Dictionary<Guid, Booking>();

            for (int i = 0; i < 10; i++)
            {
                await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken);
            }

            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = await Assert
                .ThrowsAsync<ExceededBookingLimitException>(async () => await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<ExceededBookingLimitException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddEventBooking_WhenUserAchivedBookingLimit_DoesNotAffectOtherUserLimit()
        {
            //Arrange
            var mockRepositoryLocal = new Mock<IEventRepository>();
            var loggerLocal = new Mock<ILogger<EventsService>>();
            var eventsServiceLocal = new EventsService(loggerLocal.Object, mockRepositoryLocal.Object);
            var eventsLocal = new List<Event>
                {
                    new("Event1", new DateTime(2027, 1, 14), new DateTime(2027, 1, 15), 15),
                }
                .ToDictionary(ev => ev.Id, events => events);
            mockRepositoryLocal.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(eventsLocal);

            var eventToBook = eventsLocal.FirstOrDefault().Value;

            var authLogger = new Mock<ILogger<AuthService>>();
            var config = new Mock<IConfiguration>();
            var authService = new AuthService(config.Object);
            var mockUserRepository = new Mock<IUserRepository>();
            var uLogger = new Mock<ILogger<UserService>>();
            var localUserService = new UserService(uLogger.Object, mockUserRepository.Object, authService);
            var localUsers = new List<User>
                {
                    new("User1234","12345678"),
                    new("User4321","87654321"),
                };

            //mockUserRepository.Setup(method => method.GetAllUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(localUsers);
            mockUserRepository.Setup(method => method.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken t) => localUsers.FirstOrDefault(u => u.Id == id)!);

            var user = localUsers.FirstOrDefault()!;
            var user2 = localUsers.LastOrDefault()!;
            var expectedExceptionMessage = $"User with ID {user.Id} already booked 10 events";

            var bookingRepoMock = new Mock<IBookingRepository>();
            var bookingServiceToAdd = new BookingService(bookingsLogger.Object, bookingRepoMock.Object, eventsServiceLocal, localUserService);
            var initialBookings = new Dictionary<Guid, Booking>();

            for(int i = 0; i < 10; i++)
            {
                await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken);
            }

            // Act
            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).Throws(new Exception(expectedExceptionMessage));

            var exception = await Assert
                .ThrowsAsync<ExceededBookingLimitException>(async () => await bookingServiceToAdd.CreateBookingAsync(user.Id, eventToBook.Id, TestContext.Current.CancellationToken));

            bookingRepoMock.Setup<Task<bool>>(method => method.AddBookingAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                .Callback<Booking, CancellationToken>((b, token)
                => initialBookings.Add(b.Id, b)).ReturnsAsync(true);

            var newBooking = await bookingServiceToAdd.CreateBookingAsync(user2.Id, eventToBook.Id, TestContext.Current.CancellationToken);

            // Assert
            Assert.IsType<ExceededBookingLimitException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
            Assert.Equal(user2.Id, newBooking.UserId);
            Assert.Single(user2.Bookings);
        }
    }
}
