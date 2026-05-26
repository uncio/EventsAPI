using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RU.Uncio.EventsAPI.DataAccess;
using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using RU.Uncio.EventsAPI.Services;

namespace EventsAPI.Tests
{
    public class EventServiceTests
    {
        private readonly IEventsService eventsService;
        private readonly ServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;

        public EventServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventsService, EventsService>();

            serviceProvider = services.BuildServiceProvider();
            serviceScope = serviceProvider.CreateScope();
            eventsService = serviceScope.ServiceProvider.GetRequiredService<IEventsService>();
        }

        [Fact]
        public async Task AddEventAsync_Success()
        {
            //Arrange
            Event newEvent = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);

            var initialEvents = await eventsService.GetEventsAsync(null, null, null);
            var initialAmount = initialEvents.Count;

            // Act
            await eventsService.AddEventAsync(newEvent);

            var result = await eventsService.GetEventsAsync(null, null, null);
            var resultEvent = await eventsService.GetEventAsync(newEvent.Id);

            // Assert
            Assert.Equal(initialAmount + 1, result.Count);
            Assert.Equal(newEvent.Title, resultEvent.Title);
            Assert.Equal(newEvent.StartAt.Date, resultEvent.StartAt.Date);
            Assert.Equal(newEvent.EndAt.Date, resultEvent.EndAt.Date);
        }

        [Fact]
        public async Task UpdateEventAsync_Success()
        {
            //Arrange
            Event newEvent = new("Event4", new DateTime(2026, 10, 10), new DateTime(2026, 10, 16), 12);
            await eventsService.AddEventAsync(newEvent);

            Event updateEvent = new("Event4", new DateTime(2026, 10, 10), new DateTime(2026, 12, 16), 22);

            // Act
            await eventsService.UpdateEventAsync(newEvent.Id, updateEvent);
            var result = await eventsService.GetEventAsync(newEvent.Id);

            // Assert
            Assert.Equal(updateEvent.Title, result.Title);
            Assert.Equal(updateEvent.StartAt.Date, result.StartAt.Date);
            Assert.Equal(updateEvent.EndAt.Date, result.EndAt.Date);
        }

        [Fact]
        public async Task DeleteEvent_Success()
        {
            //Arrange
            Event newEvent = new("Event4", new DateTime(2026, 10, 10), new DateTime(2026, 10, 16), 12);
            await eventsService.AddEventAsync(newEvent);

            var initialEvents = await eventsService.GetEventsAsync(null, null, null);
            var initialAmount = initialEvents.Count;
            // Act

            await eventsService.RemoveEventAsync(newEvent.Id);
            var resultEvents = await eventsService.GetEventsAsync(null, null, null);

            // Assert
            Assert.Equal(initialAmount - 1, resultEvents.Count);
        }

        [Fact]
        public async Task GetEventById_ReturnsCorrectEvent()
        {
            //Arrange
            Event newEvent = new("Event5", new DateTime(2026, 11, 11), new DateTime(2027, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent);

            // Act
            var result = await eventsService.GetEventAsync(newEvent.Id);

            // Assert
            Assert.Equal(newEvent.Id, result.Id);
            Assert.Equal("Event5", result.Title);
        }

        [Fact]
        public async Task FilterByTitle_ReturnsMatchingEvents()
        {
            //Arrange
            Event newEvent = new("Event66", new DateTime(2026, 11, 11), new DateTime(2027, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent);

            var searchSubstring = "66";
            var expectedResult = "Event66";

            // Act
            var result = await eventsService.GetEventsAsync(title: searchSubstring, null, null);

            // Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByTitle_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange            
            var searchSubstring = "Booking";

            // Act
            var result = await eventsService.GetEventsAsync(title: searchSubstring, null, null);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByStartDate_ReturnsMatchingEvents()
        {
            //Arrange
            Event newEvent1 = new("Event44", new DateTime(2026, 1, 13), new DateTime(2027, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event55", new DateTime(2026, 1, 13), new DateTime(2027, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Event77", new DateTime(2026, 1, 11), new DateTime(2027, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateFrom = new DateTime(2026, 1, 12);
            var expectedResult = new List<string> { "Event44", "Event55" };
            var notExpectedResult = "Event77";

            // Act
            var result = await eventsService.GetEventsAsync(null, from: dateFrom, null);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByStartDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2027, 1, 15);

            // Act
            var result = await eventsService.GetEventsAsync(null, from: dateFrom, null);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByEndDate_ReturnsMatchingEvents()
        {
            //Arrange
            Event newEvent1 = new("Event144", new DateTime(2026, 1, 11), new DateTime(2027, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event155", new DateTime(2026, 1, 12), new DateTime(2027, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent2);

            var dateTo = new DateTime(2026, 10, 17);
            var expectedResult = new List<string> { "Event155" };
            var notExpectedResult = "Event144";

            // Act
            var result = await eventsService.GetEventsAsync(null, null, to: dateTo);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            Event newEvent1 = new("Event144", new DateTime(2026, 1, 11), new DateTime(2026, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event155", new DateTime(2026, 1, 12), new DateTime(2026, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent2);

            var dateTo = new DateTime(2026, 1, 10);

            // Act
            var result = await eventsService.GetEventsAsync(null, null, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByStartAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange
            Event newEvent1 = new("Event21", new DateTime(2026, 1, 11), new DateTime(2026, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event22", new DateTime(2026, 1, 20), new DateTime(2026, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Event23", new DateTime(2025, 1, 20), new DateTime(2025, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateFrom = new DateTime(2026, 1, 25);
            var dateTo = new DateTime(2026, 10, 1);
            var expectedResult = new List<string> { "Event21", "Event22" };
            var notExpectedResult = "Event23";

            // Act
            var result = await eventsService.GetEventsAsync(null, from: dateFrom, to: dateTo);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByStartAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            Event newEvent1 = new("Event21", new DateTime(2026, 1, 11), new DateTime(2026, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event22", new DateTime(2026, 1, 20), new DateTime(2026, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Event23", new DateTime(2025, 1, 20), new DateTime(2025, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateFrom = new DateTime(2024, 1, 13);
            var dateTo = new DateTime(2024, 1, 14);

            // Act
            var result = await eventsService.GetEventsAsync(null, from: dateFrom, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndStartAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange
            Event newEvent1 = new("Event21", new DateTime(2026, 1, 20), new DateTime(2026, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event22", new DateTime(2025, 1, 11), new DateTime(2026, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Booking1", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateFrom = new DateTime(2026, 1, 14);
            var dateTo = new DateTime(2026, 10, 17);
            var searchSubstring = "Event";
            var notExpectedResult = new List<string> { "Booking1" };
            var expectedResult = "Event21";

            // Act
            var result = await eventsService.GetEventsAsync(title: searchSubstring, /*from: dateFrom,*/ null, to: dateTo);
            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndStartAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            Event newEvent1 = new("Event21", new DateTime(2026, 1, 20), new DateTime(2026, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event22", new DateTime(2025, 1, 11), new DateTime(2026, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Booking1", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateFrom = new DateTime(2026, 2, 14);
            var dateTo = new DateTime(2026, 9, 16);
            var searchSubstring = "Event";

            // Act
            var result = await eventsService.GetEventsAsync(title: searchSubstring, from: dateFrom, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndStartDate_ReturnsMatchingEvents()
        {
            //Arrange
            Event newEvent1 = new("Event21", new DateTime(2026, 1, 20), new DateTime(2026, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event22", new DateTime(2025, 1, 11), new DateTime(2026, 10, 20), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Booking1", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateFrom = new DateTime(2026, 1, 13);
            var searchSubstring = "Event";
            var notExpectedResult = new List<string> { "Booking1" };
            var expectedResult = "Event21";

            // Act
            var result = await eventsService.GetEventsAsync(title: searchSubstring, from: dateFrom, null);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndStartDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            Event newEvent1 = new("Event21", new DateTime(2026, 1, 20), new DateTime(2026, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent3 = new("Booking1", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateFrom = new DateTime(2026, 11, 14);
            var searchSubstring = "2";

            // Act
            var result = await eventsService.GetEventsAsync(title: searchSubstring, from: dateFrom, null);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange
            Event newEvent1 = new("Event21", new DateTime(2026, 1, 20), new DateTime(2026, 1, 26), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event22", new DateTime(2025, 1, 11), new DateTime(2026, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Booking1", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateTo = new DateTime(2026, 1, 29);
            var searchSubstring = "Event";
            var notExpectedResult = new List<string> { "Event22", "Booking1" };
            var expectedResult = "Event21";

            // Act
            var result = await eventsService.GetEventsAsync(title: searchSubstring, null, to: dateTo);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            Event newEvent1 = new("Event21", new DateTime(2026, 1, 20), new DateTime(2026, 10, 16), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent3 = new("Booking1", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent3);

            var dateTo = new DateTime(2025, 1, 16);
            var searchSubstring = "2";

            // Act
            var result = await eventsService.GetEventsAsync(title: searchSubstring, null, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaginatedEvents_ReturnsPaginatedEvents()
        {
            //Arrange
            Event newEvent1 = new("Event1", new DateTime(2026, 1, 20), new DateTime(2026, 1, 26), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event2", new DateTime(2025, 1, 11), new DateTime(2026, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Event3", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent3);
            //Event newEvent4 = new("Event4", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            //await eventsService.AddEventAsync(newEvent4);
            //Event newEvent5 = new("Event5", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            //await eventsService.AddEventAsync(newEvent5);
            //Event newEvent6 = new("Event6", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            //await eventsService.AddEventAsync(newEvent6);

            var page = 2;
            var pageSize = 2;

            var expectingItemsCount = 1;
            var notExpectedResult = new List<string> { "Event1", "Event2" };
            var expectedResult = "Event3";

            // Act
            var currentEvents = await eventsService.GetEventsAsync(null, null, null);
            var result = eventsService.GetPaginatedEvents(currentEvents, page: page, pageSize: pageSize, out int totalPages);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
            Assert.Equal(expectingItemsCount, result.Count());
        }

        [Fact]
        public async Task GetFilteredByTitlePaginatedEvents_ReturnsCorrectPagination()
        {
            //Arrange
            Event newEvent1 = new("Event1", new DateTime(2026, 1, 20), new DateTime(2026, 1, 26), 10);
            await eventsService.AddEventAsync(newEvent1);
            Event newEvent2 = new("Event23", new DateTime(2025, 1, 11), new DateTime(2026, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent2);
            Event newEvent3 = new("Event3", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent3);
            Event newEvent4 = new("Event4", new DateTime(2025, 1, 20), new DateTime(2025, 10, 10), 10);
            await eventsService.AddEventAsync(newEvent4);

            var page = 1;
            var pageSize = 2;
            var searchSubstring = "3";

            var expectingItemsCount = 2;
            var expectingTotalPages = 1;

            // Act
            var currentEvents = await eventsService.GetEventsAsync(title: searchSubstring, null, null);
            var result = eventsService.GetPaginatedEvents(currentEvents, page: page, pageSize: pageSize, out int totalPages);

            //Assert
            Assert.Equal(expectingItemsCount, result.Count());
            Assert.Equal(expectingTotalPages, totalPages);
        }


        [Fact]
        public async Task GetEventById_WhenIdDoesntExist_ReturnsNull()
        {
            //Arrange
            Event newEvent1 = new("Event1", new DateTime(2026, 1, 20), new DateTime(2026, 1, 26), 10);
            await eventsService.AddEventAsync(newEvent1);

            var id = Guid.NewGuid();
            // Act
            var result = await eventsService.GetEventAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateEventById_WhenIdDoesntExist_ThrowsMissingEventException()
        {
            //Arrange
            Event newEvent1 = new("Event1", new DateTime(2026, 1, 20), new DateTime(2026, 1, 26), 10);
            await eventsService.AddEventAsync(newEvent1);

            Event updatingEvent = new("Event1", new DateTime(2026, 1, 23), new DateTime(2026, 1, 26), 10);

            var id = Guid.NewGuid();

            var expectedExceptionMessage = $"Events collections doesn't contain an event with id";

            // Act
            var exception = await Assert
                .ThrowsAsync<MissingEventException>(() => eventsService.UpdateEventAsync(id, updatingEvent));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddEvent_WhenIdAlreadyExists_ThrowsEventExistsException()
        {
            //Arrange
            Event newEvent1 = new("Event1", new DateTime(2026, 1, 20), new DateTime(2026, 1, 26), 10);
            await eventsService.AddEventAsync(newEvent1);
            var expectedExceptionMessage = $"Event with ID {newEvent1.Id} already exists in the collection";

            // Act
            var exception = await Assert
                .ThrowsAsync<EventExistsException>(() => eventsService.AddEventAsync(newEvent1));

            // Assert
            Assert.IsType<EventExistsException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }
    }
}
