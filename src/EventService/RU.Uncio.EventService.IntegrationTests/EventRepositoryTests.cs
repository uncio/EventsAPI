using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using RU.Uncio.EventService.Domain.Models;
using RU.Uncio.EventService.Infrastructure.DataAccess;
using EventsService = RU.Uncio.EventService.Application.Services.EventsService;
using RU.Uncio.Infrastructure.Repositories;
using Testcontainers.PostgreSql;
using Xunit;

namespace RU.Uncio.EventService.IntegrationTests
{
    public class EventRepositoryTests : IAsyncLifetime
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
#pragma warning restore CS0618 // Type or member is obsolete
            .WithImage("postgres:16-alpine")
            .Build();

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.Migrate();
            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE events RESTART IDENTITY CASCADE");
        }

        [Fact]
        public async Task UpdateEvent_Success()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();

            var repository = new EventRepository(context);
            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            var t = new CancellationTokenSource();
            await repository.AddEventAsync(ev, t.Token);

            // Act
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var actEvents = await actRepository.GetEventsAsync(t.Token);
            var actEv = actEvents.Values.FirstOrDefault(e => e.Id == ev.Id);
            actEv?.Title = "Event3";
            await actRepository.UpdateEventAsync(actEv!, t.Token);

            // Assert
            await using var verifyContext = CreateContext();
            var verifyRepository = new EventRepository(verifyContext);
            var verifyEvents = await verifyRepository.GetEventsAsync(t.Token);
            var updated = verifyEvents.Values.FirstOrDefault(e => e.Id == ev.Id);
            Assert.Equal("Event3", updated?.Title);
        }

        [Fact]
        public async Task AddEvent_Success()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new EventRepository(context);
            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);

            // Act
            await repository.AddEventAsync(ev, t.Token);

            // Assert — через отдельный контекст
            await using var verifyContext = CreateContext();
            var verifyRepository = new EventRepository(verifyContext);
            var verifyEvents = await verifyRepository.GetEventsAsync(t.Token);
            var saved = verifyEvents.Values.FirstOrDefault(e => e.Id == ev.Id);

            Assert.NotNull(saved);
            Assert.Equal("Title1", saved.Title);
        }

        [Fact]
        public async Task DeleteEvent_Success()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new EventRepository(context);
            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            await repository.AddEventAsync(ev, t.Token);

            // Act
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            await actRepository.RemoveEventAsync(ev.Id, t.Token);

            // Assert
            await using var verifyContext = CreateContext();
            var verifyRepository = new EventRepository(verifyContext);
            var verifyEvents = await verifyRepository.GetEventsAsync(t.Token);
            var deleted = verifyEvents.Values.FirstOrDefault(e => e.Id == ev.Id);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsFullCollection()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new EventRepository(context);

            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            var ev2 = new Event("Title2", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);

            // Act
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var result = await actRepository.GetEventsAsync(t.Token);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Title1", result.Values.First().Title);
            Assert.Equal("Title2", result.Values.Last().Title);
        }

        [Fact]
        public async Task GetEventById_ReturnsCorrectEvent()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var t = new CancellationTokenSource();

            var repository = new EventRepository(context);
            var ev = new Event("Title1", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            await repository.AddEventAsync(ev, t.Token);

            // Act
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var actEvents = await actRepository.GetEventsAsync(t.Token);
            var result = actEvents.Values.FirstOrDefault(e => e.Id == ev.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Title1", result.Title);
        }

        [Fact]
        public async Task FilterByTitle_ReturnsMatchingAddresses()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Title21", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            var ev2 = new Event("Title2", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            var ev3 = new Event("Title3", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var searchSubstring = "2";
            var expectedResult = new List<string> { "Title21", "Title2" };
            var notExpectedResult = "Title3";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, title: searchSubstring);

            // Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByTitle_ReturnsNoEvents_WhenNoMatch()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Title21", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            var ev2 = new Event("Title2", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            var ev3 = new Event("Title3", DateTime.Now.ToUniversalTime() - TimeSpan.FromDays(1), DateTime.Now.ToUniversalTime() + TimeSpan.FromDays(1), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var searchSubstring = "4";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, title: searchSubstring);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByStartDate_ReturnsMatchingEvents()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateFrom = new DateTime(2026, 1, 14).ToUniversalTime();
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, from: dateFrom);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByStartDate_ReturnsNoEvents_WhenNoMatch()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);
            var dateFrom = new DateTime(2026, 1, 15).ToUniversalTime();

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, from: dateFrom);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByEndDate_ReturnsMatchingEvents()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateTo = new DateTime(2026, 1, 15).ToUniversalTime();
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, to: dateTo);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateTo = new DateTime(2026, 1, 14).ToUniversalTime();

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByStartAndEndDate_ReturnsMatchingEvents()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateFrom = new DateTime(2026, 1, 14).ToUniversalTime();
            var dateTo = new DateTime(2026, 1, 16).ToUniversalTime();
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, from: dateFrom, to: dateTo);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByStartAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateFrom = new DateTime(2026, 1, 13).ToUniversalTime();
            var dateTo = new DateTime(2026, 1, 14).ToUniversalTime();

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, from: dateFrom, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndStartAndEndDate_ReturnsMatchingEvents()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateFrom = new DateTime(2026, 1, 14).ToUniversalTime();
            var dateTo = new DateTime(2026, 1, 16).ToUniversalTime();
            var searchSubstring = "2";
            var notExpectedResult = new List<string> { "Event1", "Event22" };
            var expectedResult = "Event2";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, title: searchSubstring, from: dateFrom, to: dateTo);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndStartAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateFrom = new DateTime(2026, 1, 14).ToUniversalTime();
            var dateTo = new DateTime(2026, 1, 16).ToUniversalTime();
            var searchSubstring = "3";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, title: searchSubstring, from: dateFrom, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndStartDate_ReturnsMatchingEvents()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateFrom = new DateTime(2026, 1, 13).ToUniversalTime();
            var searchSubstring = "1";
            var notExpectedResult = new List<string> { "Event2", "Event22" };
            var expectedResult = "Event1";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, title: searchSubstring, from: dateFrom);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndStartDate_ReturnsNoEvents_WhenNoMatch()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateFrom = new DateTime(2026, 1, 14).ToUniversalTime();
            var searchSubstring = "3";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, title: searchSubstring, from: dateFrom);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndEndDate_ReturnsMatchingEvents()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateTo = new DateTime(2026, 1, 15).ToUniversalTime();
            var searchSubstring = "1";
            var notExpectedResult = new List<string> { "Event2", "Event22" };
            var expectedResult = "Event1";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, title: searchSubstring, to: dateTo);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);

            var dateTo = new DateTime(2026, 1, 16).ToUniversalTime();
            var searchSubstring = "3";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);
            var result = await eventService.GetEventsAsync(t.Token, title: searchSubstring, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaginatedEvents_ReturnsPaginatedEvents()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);
            //Arrange           
            var page = 2;
            var pageSize = 2;

            var expectingItemsCount = 1;
            var notExpectedResult = new List<string> { "Event1", "Event2" };
            var expectedResult = "Event22";

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);

            var currentEvents = await eventService.GetEventsAsync(t.Token);
            var result = eventService.GetPaginatedEvents(currentEvents, page: page, pageSize: pageSize, out int totalPages);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
            Assert.Equal(expectingItemsCount, result.Count());
        }

        [Fact]
        public async Task GetFilteredByTitlePaginatedEvents_ReturnsCorrectPagination()
        {
            await ResetDatabaseAsync();

            // Arrange
            await using var context = CreateContext();
            var repository = new EventRepository(context);
            var t = new CancellationTokenSource();

            var ev = new Event("Event1", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev2 = new Event("Event2", new DateTime(2026, 1, 14).ToUniversalTime(), new DateTime(2026, 1, 15).ToUniversalTime(), 10);
            var ev3 = new Event("Event22", new DateTime(2026, 1, 13).ToUniversalTime(), new DateTime(2026, 1, 16).ToUniversalTime(), 10);
            await repository.AddEventAsync(ev, t.Token);
            await repository.AddEventAsync(ev2, t.Token);
            await repository.AddEventAsync(ev3, t.Token);
            //Arrange           
            var page = 1;
            var pageSize = 2;
            var searchSubstring = "2";

            var expectingItemsCount = 2;
            var expectingTotalPages = 1;

            // Act
            var logger = new Mock<ILogger<RU.Uncio.EventService.Application.Services.EventsService>>();
            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var eventService = new RU.Uncio.EventService.Application.Services.EventsService(logger.Object, actRepository);

            var currentEvents = await eventService.GetEventsAsync(t.Token, title: searchSubstring);
            var result = eventService.GetPaginatedEvents(currentEvents, page: page, pageSize: pageSize, out int totalPages);

            //Assert
            Assert.Equal(expectingItemsCount, result.Count());
            Assert.Equal(expectingTotalPages, totalPages);
        }
    }
}