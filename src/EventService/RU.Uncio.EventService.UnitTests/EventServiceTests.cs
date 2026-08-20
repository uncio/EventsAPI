using Microsoft.Extensions.Logging;
using Moq;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Application.Services;
using RU.Uncio.EventService.Domain.Exceptions;
using RU.Uncio.EventService.Domain.Models;
using System.Net;

namespace RU.Uncio.EventService.UnitTests
{
    public class EventServiceTests
    {
        private readonly EventsService eventsService;
        private readonly Dictionary<Guid, Event> events;
        private readonly Mock<ILogger<EventsService>> logger;

        public EventServiceTests()
        {
            var mockRepository = new Mock<IEventRepository>();
            var mockCacheRepository = new Mock<IEventCacheRepository>();
            logger = new Mock<ILogger<EventsService>>();
            eventsService = new EventsService(logger.Object, mockRepository.Object, mockCacheRepository.Object);
            events = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event22",new DateTime(2026, 1, 13), new DateTime(2026, 1, 16), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);

            mockRepository.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(events);
        }

        [Fact]
        public async Task AddEvent_Success()
        {
            //Arrange
            var mockRepositoryToAdd = new Mock<IEventRepository>();
            var mockCacheRepositoryToAdd = new Mock<IEventCacheRepository>();
            var eventsServiceToAdd = new EventsService(logger.Object, mockRepositoryToAdd.Object, mockCacheRepositoryToAdd.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);
            Event newEvent = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);

            mockRepositoryToAdd.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialEvents);
            mockRepositoryToAdd.Setup(method => method.AddEventAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .Callback<Event, CancellationToken>((ev, token) => initialEvents.Add(ev.Id, ev));

            // Act
            await eventsServiceToAdd.AddEventAsync(newEvent, TestContext.Current.CancellationToken);
            var result = await eventsServiceToAdd.GetEventsAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(newEvent.Title, result.Last().Title);
            Assert.Equal(newEvent.StartAt.Date, result.Last().StartAt.Date);
            Assert.Equal(newEvent.EndAt.Date, result.Last().EndAt.Date);
        }

        [Fact]
        public async Task UpdateEvent_Success_CacheUpdated()
        {
            //Arrange
            var mockRepositoryToUpdate = new Mock<IEventRepository>();
            var mockCacheRepositoryToAdd = new Mock<IEventCacheRepository>();
            var eventsServiceToUpdate = new EventsService(logger.Object, mockRepositoryToUpdate.Object, mockCacheRepositoryToAdd.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);
            Event updatingEvent = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            var idToUpdate = initialEvents.Keys.LastOrDefault();

            mockRepositoryToUpdate.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialEvents);
            mockRepositoryToUpdate.Setup(method => method.UpdateEventAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .Callback<Event, CancellationToken>((ev, token) =>
            {
                initialEvents[ev.Id].Title = ev.Title;
                initialEvents[ev.Id].Description = ev.Description;
                initialEvents[ev.Id].StartAt = ev.StartAt;
                initialEvents[ev.Id].EndAt = ev.EndAt;
            });

            // Act
            await eventsServiceToUpdate.UpdateEventAsync(idToUpdate, updatingEvent, TestContext.Current.CancellationToken);
            var result = await eventsServiceToUpdate.GetEventsAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(updatingEvent.Title, result.Last().Title);
            Assert.Equal(updatingEvent.StartAt.Date, result.Last().StartAt.Date);
            Assert.Equal(updatingEvent.EndAt.Date, result.Last().EndAt.Date);
            mockCacheRepositoryToAdd.Verify(repository => repository.SetAsync(updatingEvent), Times.Once);
        }

        [Fact]
        public async Task DeleteEvent_Success()
        {
            //Arrange
            var mockRepositoryToDelete = new Mock<IEventRepository>();
            var mockCacheRepositoryToAdd = new Mock<IEventCacheRepository>();
            var eventsServiceToDelete = new EventsService(logger.Object, mockRepositoryToDelete.Object, mockCacheRepositoryToAdd.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);

            var idToDelete = initialEvents.Keys.LastOrDefault();

            mockRepositoryToDelete.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialEvents);
            mockRepositoryToDelete.Setup(method => method.RemoveEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, CancellationToken>((id, token) => initialEvents.Remove(id));

            // Act
            await eventsServiceToDelete.RemoveEventAsync(idToDelete, TestContext.Current.CancellationToken);
            var result = await eventsServiceToDelete.GetEventsAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsFullCollection()
        {
            //Arrange

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Event1", result.First().Title);
            Assert.Equal("Event22", result.Last().Title);
        }

        [Fact]
        public async Task GetEventById_ReturnsCorrectEventFromCache()
        {
            //Arrange
            var id = events.Keys.ToList()[1];
            var mockRepository = new Mock<IEventRepository>();
            var mockCacheRepository = new Mock<IEventCacheRepository>();

            mockCacheRepository.Setup(method => method.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(events.FirstOrDefault(ev => ev.Key == id).Value);
            var logger = new Mock<ILogger<EventsService>>();

            var eventsService = new EventsService(logger.Object, mockRepository.Object, mockCacheRepository.Object);
            // Act
            var result = await eventsService.GetEventAsync(id, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal("Event2", result.Title);
            mockRepository.Verify(repository => repository.GetEventAsync(id, TestContext.Current.CancellationToken), Times.Never);
        }

        [Fact]
        public async Task GetEventById_WhenNotInCahce_ReturnsCorrectEventFromRepositoryAndSaveInCache()
        {
            //Arrange
            var ev = events.Values.ToList()[1];
            var id = ev.Id;
            var mockRepository = new Mock<IEventRepository>();

            mockRepository.Setup(method => method.GetEventAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events.FirstOrDefault(ev => ev.Key == id).Value);
            var logger = new Mock<ILogger<EventsService>>();
            var mockCacheRepositoryToAdd = new Mock<IEventCacheRepository>();
            var eventsService = new EventsService(logger.Object, mockRepository.Object, mockCacheRepositoryToAdd.Object);


            // Act
            var result = await eventsService.GetEventAsync(id, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal("Event2", result.Title);
            mockRepository.Verify(repository => repository.GetEventAsync(id, TestContext.Current.CancellationToken), Times.Once);
            mockCacheRepositoryToAdd.Verify(repository => repository.SetAsync(ev), Times.Once);
        }

        [Fact]
        public async Task FilterByTitle_ReturnsMatchingAddresses()
        {
            //Arrange
            var searchSubstring = "2";
            var expectedResult = new List<string> { "Event2", "Event22" };
            var notExpectedResult = "Event1";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring);

            // Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByTitle_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange            
            var searchSubstring = "3";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByStartDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateFrom = new DateTime(2026, 1, 14);
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, from: dateFrom);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByStartDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2026, 1, 15);

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, from: dateFrom);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByEndDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateTo = new DateTime(2026, 1, 15);
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, to: dateTo);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateTo = new DateTime(2026, 1, 14);

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByStartAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateFrom = new DateTime(2026, 1, 14);
            var dateTo = new DateTime(2026, 1, 16);
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, from: dateFrom, to: dateTo);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public async Task FilterByStartAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2026, 1, 13);
            var dateTo = new DateTime(2026, 1, 14);

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, from: dateFrom, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndStartAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateFrom = new DateTime(2026, 1, 14);
            var dateTo = new DateTime(2026, 1, 16);
            var searchSubstring = "2";
            var notExpectedResult = new List<string> { "Event1", "Event22" };
            var expectedResult = "Event2";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring, from: dateFrom, to: dateTo);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndStartAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2026, 1, 14);
            var dateTo = new DateTime(2026, 1, 16);
            var searchSubstring = "3";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring, from: dateFrom, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndStartDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateFrom = new DateTime(2026, 1, 13);
            var searchSubstring = "1";
            var notExpectedResult = new List<string> { "Event2", "Event22" };
            var expectedResult = "Event1";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring, from: dateFrom);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndStartDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2026, 1, 14);
            var searchSubstring = "3";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring, from: dateFrom);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByTitleAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateTo = new DateTime(2026, 1, 15);
            var searchSubstring = "1";
            var notExpectedResult = new List<string> { "Event2", "Event22" };
            var expectedResult = "Event1";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring, to: dateTo);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public async Task FilterByTitleAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateTo = new DateTime(2026, 1, 16);
            var searchSubstring = "3";

            // Act
            var result = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaginatedEvents_ReturnsPaginatedEvents()
        {
            //Arrange           
            var page = 2;
            var pageSize = 2;

            var expectingItemsCount = 1;
            var notExpectedResult = new List<string> { "Event1", "Event2" };
            var expectedResult = "Event22";

            // Act
            var currentEvents = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken);
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
            var page = 1;
            var pageSize = 2;
            var searchSubstring = "2";

            var expectingItemsCount = 2;
            var expectingTotalPages = 1;

            // Act
            var currentEvents = await eventsService.GetEventsAsync(TestContext.Current.CancellationToken, title: searchSubstring);
            var result = eventsService.GetPaginatedEvents(currentEvents, page: page, pageSize: pageSize, out int totalPages);

            //Assert
            Assert.Equal(expectingItemsCount, result.Count());
            Assert.Equal(expectingTotalPages, totalPages);
        }


        [Fact]
        public async Task GetEventById_WhenIdDoesntExist_ReturnsNull()
        {
            //Arrange
            var id = Guid.NewGuid();
            // Act
            var result = await eventsService.GetEventAsync(id, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateEventById_WhenIdDoesntExist_ThrowsMissingEventException()
        {
            //Arrange
            var expectedExceptionMessage = $"Events collections doesn't contain an event with id";
            var mockRepositoryToUpdate = new Mock<IEventRepository>();
            var mockCacheRepositoryToAdd = new Mock<IEventCacheRepository>();
            var eventsServiceToUpdate = new EventsService(logger.Object, mockRepositoryToUpdate.Object, mockCacheRepositoryToAdd.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);
            Event updatingEvent = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16), 10);
            var idToUpdate = Guid.NewGuid();

            mockRepositoryToUpdate.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialEvents);
            mockRepositoryToUpdate.Setup(method => method.UpdateEventAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = await Assert
                .ThrowsAsync<MissingEventException>(() => eventsServiceToUpdate.UpdateEventAsync(idToUpdate, updatingEvent, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task AddEvent_WhenIdAlreadyExists_ThrowsEventExistsException()
        {
            //Arrange            
            var mockRepositoryToUpdate = new Mock<IEventRepository>();
            var mockCacheRepositoryToAdd = new Mock<IEventCacheRepository>();
            var eventsServiceToUpdate = new EventsService(logger.Object, mockRepositoryToUpdate.Object, mockCacheRepositoryToAdd.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15), 10),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16), 10),
                }
                .ToDictionary(ev => ev.Id, events => events);
            Event addingEvent = initialEvents.FirstOrDefault().Value;
            var expectedExceptionMessage = $"Event with ID {addingEvent.Id} already exists in the collection";

            mockRepositoryToUpdate.Setup(method => method.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(initialEvents);
            mockRepositoryToUpdate.Setup(method => method.AddEventAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = await Assert
                .ThrowsAsync<EventExistsException>(() => eventsServiceToUpdate.AddEventAsync(addingEvent, TestContext.Current.CancellationToken));

            // Assert
            Assert.IsType<EventExistsException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }
    }   
}
