
using Microsoft.Extensions.Logging;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Domain.Exceptions;
using RU.Uncio.EventService.Domain.Models;

namespace RU.Uncio.EventService.Application.Services
{
    /// <summary>
    /// Service to manipulate with events collection
    /// </summary>
    public class EventsService : IEventsService
    {
        private readonly ILogger<EventsService> logger;
        private readonly IEventRepository repository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="repo"></param>
        public EventsService(ILogger<EventsService> log, IEventRepository repo)
        {
            logger = log;
            repository = repo;
        }

        /// <summary>
        /// Gets all events from collection
        /// </summary>
        /// <param name="token">cancellation token</param>
        /// <param name="title">Title filter</param>
        /// <param name="from">Event starts from filter</param>
        /// <param name="to">Event ends up to filter</param>
        /// <returns>Collection of filtered events</returns>
        public async Task<List<Event>> GetEventsAsync(CancellationToken token, string? title = null, DateTime? from = null, DateTime? to = null)
        {
            var result = await repository.GetEventsAsync(token);
            IEnumerable<Event> events = result.Values.ToList();

            if (!String.IsNullOrEmpty(title))
            {
                var lowerTitleFilter = title.ToLower();
                events = events
                    .Where(ev => ev.Title.ToLower().Contains(lowerTitleFilter));
            }

            if(from != null)
            {
                events = events
                    .Where(ev => ev.StartAt.Date >= from.Value.Date);
            }

            if(to != null)
            {
                events = events
                    .Where(ev => to.Value.Date >= ev.EndAt.Date);
            }

            return events.ToList();
        }

        /// <summary>
        /// Returns paginated events
        /// </summary>
        /// <param name="filtered">events after filtering</param>
        /// <param name="page">page number</param>
        /// <param name="pageSize">items number per page</param>
        /// <param name="totalPages">calculated amount of pages</param>
        /// <returns></returns>
        public List<Event> GetPaginatedEvents(IEnumerable<Event> filtered, int page, int pageSize, out int totalPages)
        {
            var items = filtered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

            totalPages = (int)Math.Ceiling((double)filtered.Count() / pageSize);

            return items.ToList();
        }

        /// <summary>
        /// Gets an event from collection by ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="token">cancellation token</param>
        /// <returns></returns>
        public async Task<Event> GetEventAsync(Guid id, CancellationToken token)
        {
            var events = await repository.GetEventsAsync(token);

            if (events.TryGetValue(id, out var ev))
                return ev;

            logger.LogError($"Events collections doesn't contain an event with id {id}");
            return null;
        }

        /// <summary>
        /// Adds an event to collection
        /// </summary>
        /// <param name="ev">Event to add</param>
        /// <param name="token">cancellation token</param>
        /// <exception cref="ArgumentException"></exception>
        public async Task AddEventAsync(Event ev, CancellationToken token)
        {
            var events = await repository.GetEventsAsync(token);

            if (!events.TryGetValue(ev.Id, out var @event))
                await repository.AddEventAsync(ev, token);
            else
            {
                throw new EventExistsException($"Event with ID {ev.Id} already exists in the collection");
            }
        }

        /// <summary>
        /// Updates an event i collection by ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="ev">Event to update</param>
        /// <param name="token">cancellation token</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public async Task UpdateEventAsync(Guid id, Event ev, CancellationToken token)
        {
            var events = await repository.GetEventsAsync(token);

            if (events.TryGetValue(id, out var currentEvent))
            {
                if (currentEvent.TotalSeats - currentEvent.AvailableSeats > ev.TotalSeats)
                {
                    throw new TotalGreaterBookedException($"Not possible to change total seats. Amount of bookings for the event is greater than new total seats value");
                }
                else
                {
                    currentEvent.UpdateWith(ev);
                    await repository.UpdateEventAsync(currentEvent, token);
                }
            }
            else
            {
                throw new MissingEventException($"Events collections doesn't contain an event with id {id}");
            }
        }

        /// <summary>
        /// Removes an event from collection by ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="token">cancellation token</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public async Task RemoveEventAsync(Guid id, CancellationToken token)
        {
            var events = await repository.GetEventsAsync(token);

            if (events.TryGetValue(id, out var _))
            {
                await repository.RemoveEventAsync(id, token);
            }
            else
            {
                throw new MissingEventException($"Events collections doesn't contain an event with id {id}");
            }
        }
    }
}
