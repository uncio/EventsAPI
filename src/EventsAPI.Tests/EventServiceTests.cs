using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using RU.Uncio.EventsAPI.Services;

namespace EventsAPI.Tests
{
    public class EventServiceTests
    {
        private readonly EventsService eventsService;
        private readonly Dictionary<Guid, Event> events;
        private readonly Mock<ILogger<EventsService>> logger;

        public EventServiceTests()
        {
            var mockRepository = new Mock<IEventRepository>();
            logger = new Mock<ILogger<EventsService>>();
            eventsService = new EventsService(logger.Object, mockRepository.Object);
            events = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 13), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);

            mockRepository.Setup(method => method.GetEvents()).Returns(events);
        }

        [Fact]
        public void AddEvent_Success()
        {
            //Arrange
            var mockRepositoryToAdd = new Mock<IEventRepository>();
            var eventsServiceToAdd = new EventsService(logger.Object, mockRepositoryToAdd.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);
            Event newEvent = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16));

            mockRepositoryToAdd.Setup(method => method.GetEvents()).Returns(initialEvents);
            mockRepositoryToAdd.Setup(method => method.AddEvent(It.IsAny<Event>())).Callback<Event>((ev) => initialEvents.Add(ev.Id, ev));

            // Act
            eventsServiceToAdd.AddEvent(newEvent);
            var result = eventsServiceToAdd.GetEvents();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(newEvent.Title, result.Last().Title);
            Assert.Equal(newEvent.StartAt.Date, result.Last().StartAt.Date);
            Assert.Equal(newEvent.EndAt.Date, result.Last().EndAt.Date);
        }

        [Fact]
        public void UpdateEvent_Success()
        {
            //Arrange
            var mockRepositoryToUpdate = new Mock<IEventRepository>();
            var eventsServiceToUpdate = new EventsService(logger.Object, mockRepositoryToUpdate.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);
            Event updatingEvent = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16));
            var idToUpdate = initialEvents.Keys.LastOrDefault();

            mockRepositoryToUpdate.Setup(method => method.GetEvents()).Returns(initialEvents);
            mockRepositoryToUpdate.Setup(method => method.UpdateEvent(It.IsAny<Guid>(), It.IsAny<Event>())).Callback<Guid, Event>((id, ev) =>
            {
                initialEvents[id].Title = ev.Title;
                initialEvents[id].Description = ev.Description;
                initialEvents[id].StartAt = ev.StartAt;
                initialEvents[id].EndAt = ev.EndAt;
            });

            // Act
            eventsServiceToUpdate.UpdateEvent(idToUpdate, updatingEvent);
            var result = eventsServiceToUpdate.GetEvents();

            // Assert
            Assert.Equal(updatingEvent.Title, result.Last().Title);
            Assert.Equal(updatingEvent.StartAt.Date, result.Last().StartAt.Date);
            Assert.Equal(updatingEvent.EndAt.Date, result.Last().EndAt.Date);
        }

        [Fact]
        public void DeleteEvent_Success()
        {
            //Arrange
            var mockRepositoryToDelete = new Mock<IEventRepository>();
            var eventsServiceToDelete = new EventsService(logger.Object, mockRepositoryToDelete.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);

            var idToDelete = initialEvents.Keys.LastOrDefault();

            mockRepositoryToDelete.Setup(method => method.GetEvents()).Returns(initialEvents);
            mockRepositoryToDelete.Setup(method => method.RemoveEvent(It.IsAny<Guid>())).Callback<Guid>((id) => initialEvents.Remove(id));

            // Act
            eventsServiceToDelete.RemoveEvent(idToDelete);
            var result = eventsServiceToDelete.GetEvents();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetAllEvents_ReturnsFullCollection()
        {
            //Arrange

            // Act
            var result = eventsService.GetEvents();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Event1", result.First().Title);
            Assert.Equal("Event22", result.Last().Title);
        }

        [Fact]
        public void GetEventById_ReturnsCorrectEvent()
        {
            //Arrange
            var id = events.Keys.ToList()[1];
            // Act
            var result = eventsService.GetEvent(id);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal("Event2", result.Title);
        }

        [Fact]
        public void FilterByTitle_ReturnsMatchingAddresses()
        {
            //Arrange
            var searchSubstring = "2";
            var expectedResult = new List<string> { "Event2", "Event22" };
            var notExpectedResult = "Event1";

            // Act
            var result = eventsService.GetEvents(title: searchSubstring);

            // Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public void FilterByTitle_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange            
            var searchSubstring = "3";

            // Act
            var result = eventsService.GetEvents(title: searchSubstring);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FilterByStartDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateFrom = new DateTime(2026, 1, 14);
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var result = eventsService.GetEvents(from: dateFrom);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public void FilterByStartDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2026, 1, 15);

            // Act
            var result = eventsService.GetEvents(from: dateFrom);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FilterByEndDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateTo = new DateTime(2026, 1, 15);
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var result = eventsService.GetEvents(to: dateTo);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public void FilterByEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateTo = new DateTime(2026, 1, 14);

            // Act
            var result = eventsService.GetEvents(to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FilterByStartAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateFrom = new DateTime(2026, 1, 14);
            var dateTo = new DateTime(2026, 1, 16);
            var expectedResult = new List<string> { "Event1", "Event2" };
            var notExpectedResult = "Event22";

            // Act
            var result = eventsService.GetEvents(from: dateFrom, to: dateTo);

            //Assert
            Assert.All(result, ev => expectedResult.Contains(ev.Title));
            Assert.DoesNotContain(notExpectedResult, result.Select(ev => ev.Title));
        }

        [Fact]
        public void FilterByStartAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2026, 1, 13);
            var dateTo = new DateTime(2026, 1, 14);

            // Act
            var result = eventsService.GetEvents(from: dateFrom, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FilterByTitleAndStartAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateFrom = new DateTime(2026, 1, 14);
            var dateTo = new DateTime(2026, 1, 16);
            var searchSubstring = "2";
            var notExpectedResult = new List<string> { "Event1", "Event22" };
            var expectedResult = "Event2";

            // Act
            var result = eventsService.GetEvents(title: searchSubstring, from: dateFrom, to: dateTo);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public void FilterByTitleAndStartAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2026, 1, 14);
            var dateTo = new DateTime(2026, 1, 16);
            var searchSubstring = "3";

            // Act
            var result = eventsService.GetEvents(title: searchSubstring, from: dateFrom, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FilterByTitleAndStartDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateFrom = new DateTime(2026, 1, 13);
            var searchSubstring = "1";
            var notExpectedResult = new List<string> { "Event2", "Event22" };
            var expectedResult = "Event1";

            // Act
            var result = eventsService.GetEvents(title: searchSubstring, from: dateFrom);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public void FilterByTitleAndStartDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateFrom = new DateTime(2026, 1, 14);
            var searchSubstring = "3";

            // Act
            var result = eventsService.GetEvents(title: searchSubstring, from: dateFrom);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FilterByTitleAndEndDate_ReturnsMatchingEvents()
        {
            //Arrange           
            var dateTo = new DateTime(2026, 1, 15);
            var searchSubstring = "1";
            var notExpectedResult = new List<string> { "Event2", "Event22" };
            var expectedResult = "Event1";

            // Act
            var result = eventsService.GetEvents(title: searchSubstring, to: dateTo);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
        }

        [Fact]
        public void FilterByTitleAndEndDate_ReturnsNoEvents_WhenNoMatch()
        {
            //Arrange
            var dateTo = new DateTime(2026, 1, 16);
            var searchSubstring = "3";

            // Act
            var result = eventsService.GetEvents(title: searchSubstring, to: dateTo);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPaginatedEvents_ReturnsPaginatedEvents()
        {
            //Arrange           
            var page = 2;
            var pageSize = 2;

            var expectingItemsCount = 1;
            var notExpectedResult = new List<string> { "Event1", "Event2" };
            var expectedResult = "Event22";

            // Act
            var currentEvents = eventsService.GetEvents();
            var result = eventsService.GetPaginatedEvents(currentEvents, page: page, pageSize: pageSize, out int totalPages);

            //Assert
            Assert.Contains(expectedResult, result.Select(ev => ev.Title));
            Assert.DoesNotContain(result, ev => notExpectedResult.Contains(ev.Title));
            Assert.Equal(expectingItemsCount, result.Count());
        }

        [Fact]
        public void GetFilteredByTitlePaginatedEvents_ReturnsCorrectPagination()
        {
            //Arrange           
            var page = 1;
            var pageSize = 2;
            var searchSubstring = "2";

            var expectingItemsCount = 2;
            var expectingTotalPages = 1;

            // Act
            var currentEvents = eventsService.GetEvents(title: searchSubstring);
            var result = eventsService.GetPaginatedEvents(currentEvents, page: page, pageSize: pageSize, out int totalPages);

            //Assert
            Assert.Equal(expectingItemsCount, result.Count());
            Assert.Equal(expectingTotalPages, totalPages);
        }


        [Fact]
        public void GetEventById_WhenIdDoesntExist_ReturnsNull()
        {
            //Arrange
            var id = Guid.NewGuid();
            // Act
            var result = eventsService.GetEvent(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void UpdateEventById_WhenIdDoesntExist_ThrowsMissingEventException()
        {
            //Arrange
            var expectedExceptionMessage = $"Events collections doesn't contain an event with id";
            var mockRepositoryToUpdate = new Mock<IEventRepository>();
            var eventsServiceToUpdate = new EventsService(logger.Object, mockRepositoryToUpdate.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);
            Event updatingEvent = new("Event3", new DateTime(2026, 1, 1), new DateTime(2026, 1, 16));
            var idToUpdate = Guid.NewGuid();

            mockRepositoryToUpdate.Setup(method => method.GetEvents()).Returns(initialEvents);
            mockRepositoryToUpdate.Setup(method => method.UpdateEvent(It.IsAny<Guid>(), It.IsAny<Event>()))
                .Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = Assert
                .Throws<MissingEventException>(() => eventsServiceToUpdate.UpdateEvent(idToUpdate, updatingEvent));

            // Assert
            Assert.IsType<MissingEventException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }

        [Fact]
        public void AddEvent_WhenIdAlreadyExists_ThrowsEventExistsException()
        {
            //Arrange            
            var mockRepositoryToUpdate = new Mock<IEventRepository>();
            var eventsServiceToUpdate = new EventsService(logger.Object, mockRepositoryToUpdate.Object);
            var initialEvents = new List<Event>
                {
                    new("Event1", new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event2",new DateTime(2026, 1, 14), new DateTime(2026, 1, 15)),
                    new("Event22",new DateTime(2026, 1, 15), new DateTime(2026, 1, 16)),
                }
                .ToDictionary(ev => ev.Id, events => events);
            Event addingEvent = initialEvents.FirstOrDefault().Value;
            var expectedExceptionMessage = $"Event with ID {addingEvent.Id} already exists in the collection";

            mockRepositoryToUpdate.Setup(method => method.GetEvents()).Returns(initialEvents);
            mockRepositoryToUpdate.Setup(method => method.AddEvent(It.IsAny<Event>()))
                .Throws(new Exception(expectedExceptionMessage));

            // Act
            var exception = Assert
                .Throws<EventExistsException>(() => eventsServiceToUpdate.AddEvent(addingEvent));

            // Assert
            Assert.IsType<EventExistsException>(exception);
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }
    }
}
