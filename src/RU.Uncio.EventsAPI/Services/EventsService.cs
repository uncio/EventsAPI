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

        public EventsService(ILogger<EventsService> log)
        {
            logger = log;
        }

        /// <summary>
        /// Collection of events
        /// </summary>
        public static Dictionary<Guid, Event> Events { get; private set; } = [];

        /// <summary>
        /// Gets all events from collection
        /// </summary>
        /// <param name="title">Title filter</param>
        /// <param name="from">Event starts from filter</param>
        /// <param name="to">Event ends up to filter</param>
        /// <returns>Collection of filtered events</returns>
        public List<Event> GetEvents(string? title, DateTime? from, DateTime? to)
        {
            IEnumerable<Event> result = Events.Values.ToList();

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
                    Events.Count()
                );
        }

        /// <summary>
        /// Gets an event from collection by ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <returns></returns>
        public Event GetEvent(Guid id)
        {
            if(Events.TryGetValue(id, out var ev))
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
            if(!Events.ContainsKey(ev.Id))
                Events[ev.Id] = ev;
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
            if (Events.TryGetValue(id, out _))
            {                
                Events[id].UpdateWith(ev);
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
            if (Events.TryGetValue(id, out _))
            {
                Events.Remove(id);
            }
            else
            {
                throw new MissingEventException($"Events collections doesn't contain an event with id {id}");
            }
        }
    }
}
