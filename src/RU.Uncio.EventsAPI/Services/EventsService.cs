using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
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
        /// <param name="title">Title filter</param>
        /// <param name="from">Event starts from filter</param>
        /// <param name="to">Event ends up to filter</param>
        /// <returns>Collection of filtered events</returns>
        public List<Event> GetEvents(string? title = null, DateTime? from = null, DateTime? to = null)
        {
            IEnumerable<Event> result = repository.GetEvents().Values.ToList();

            if (!String.IsNullOrEmpty(title))
            {
                var lowerTitleFilter = title.ToLower();
                result = result
                    .Where(ev => ev.Title.ToLower().Contains(lowerTitleFilter));
            }

            if(from != null)
            {
                result = result
                    .Where(ev => ev.StartAt.Date >= from.Value.Date);
            }

            if(to != null)
            {
                result = result
                    .Where(ev => to.Value.Date >= ev.EndAt.Date);
            }

            return result.ToList();
        }

        /// <summary>
        /// Returns paginated events
        /// </summary>
        /// <param name="filtered">events after filtering</param>
        /// <param name="page">page number</param>
        /// <param name="pageSize">items number per page</param>
        /// <returns></returns>
        public PaginatedResultDTO<EventDTO> GetPaginatedEvents(IEnumerable<EventDTO> filtered, int page, int pageSize)
        {
            var items = filtered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

            int totalPages = (int)Math.Ceiling((double)filtered.Count() / pageSize);

            return new PaginatedResultDTO<EventDTO>
                (
                    items.ToList(),
                    items.Count(),
                    page,
                    totalPages,
                    repository.GetEvents().Count()
                );
        }

        /// <summary>
        /// Gets an event from collection by ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <returns></returns>
        public Event GetEvent(Guid id)
        {
            if(repository.GetEvents().TryGetValue(id, out var ev))
                return ev;

            logger.LogError($"Events collections doesn't contain an event with id {id}");
            return null;
        }
        /// <summary>
        /// Adds an event to collection
        /// </summary>
        /// <param name="ev">Event to add</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddEvent(Event ev)
        {
            if(!repository.GetEvents().ContainsKey(ev.Id))
                repository.AddEvent(ev);
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
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void UpdateEvent(Guid id, Event ev)
        {
            if (repository.GetEvents().TryGetValue(id, out _))
            {                
                repository.UpdateEvent(id, ev);
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
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void RemoveEvent(Guid id)
        {
            if (repository.GetEvents().TryGetValue(id, out _))
            {
                repository.RemoveEvent(id);
            }
            else
            {
                throw new MissingEventException($"Events collections doesn't contain an event with id {id}");
            }
        }
    }
}
