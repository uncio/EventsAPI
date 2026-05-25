using RU.Uncio.EventsAPI.DataAccess;
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
        //private readonly IEventRepository repository;
        private AppDbContext appDbContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="repo"></param>
        public EventsService(ILogger<EventsService> log, /*IEventRepository repo,*/ AppDbContext context)
        {
            logger = log;
            //repository = repo;
            appDbContext = context;
        }

        /// <summary>
        /// Gets all events from collection
        /// </summary>
        /// <param name="title">Title filter</param>
        /// <param name="from">Event starts from filter</param>
        /// <param name="to">Event ends up to filter</param>
        /// <returns>Collection of filtered events</returns>
        public async Task<List<Event>> GetEventsAsync(string? title = null, DateTime? from = null, DateTime? to = null)
        {
            IEnumerable<Event> result = appDbContext.Events;//repository.GetEvents().Values.ToList();

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
        /// <returns></returns>
        public async Task<Event> GetEventAsync(Guid id)
        {
            //if(repository.GetEvents().TryGetValue(id, out var ev))
            //    return ev;

            var result = appDbContext.Events.FirstOrDefault(ev => ev.Id == id);

            if(result != null)
                return result;

            logger.LogError($"Events collections doesn't contain an event with id {id}");
            return null;
        }
        /// <summary>
        /// Adds an event to collection
        /// </summary>
        /// <param name="ev">Event to add</param>
        /// <exception cref="ArgumentException"></exception>
        public async Task AddEventAsync(Event ev)
        {
            var existingEvent = appDbContext.Events.FirstOrDefault(e => e.Id == ev.Id);

            //if (!repository.GetEvents().ContainsKey(ev.Id))
            //    repository.AddEvent(ev);
            if (existingEvent == null)
            {
                await appDbContext.AddAsync(ev);
                await appDbContext.SaveChangesAsync();
            }
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
        public async Task UpdateEventAsync(Guid id, Event ev)
        {
            var currentEvent = appDbContext.Events.FirstOrDefault(e => e.Id == ev.Id);

            if (currentEvent != null)
            {
                if(currentEvent.TotalSeats - currentEvent.AvailableSeats > ev.TotalSeats)
                {
                    throw new TotalGreaterBookedException($"Not possible to change total seats. Amount of bookings for the event is greater than new total seats value");
                }
                else
                {
                    currentEvent.UpdateWith(ev);
                    appDbContext.Update(currentEvent);
                    await appDbContext.SaveChangesAsync();
                }                
            }
            //if (repository.GetEvents().TryGetValue(id, out var currentEvent))
            //{
            //    if(currentEvent.TotalSeats - currentEvent.AvailableSeats > ev.TotalSeats)
            //    {
            //        throw new TotalGreaterBookedException($"Not possible to change total seats. Amount of bookings for the event is greater than new total seats value");
            //    }
            //    else
            //    {
            //        repository.UpdateEvent(id, ev);
            //    }                
            //}
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
        public async Task RemoveEventAsync(Guid id)
        {
            var currentEvent = appDbContext.Events.FirstOrDefault(e => e.Id == ev.Id);

            if (currentEvent != null)
            {
                appDbContext.Remove(currentEvent);
                await appDbContext.SaveChangesAsync();
            }
            //if (repository.GetEvents().TryGetValue(id, out _))
            //{
            //    repository.RemoveEvent(id);
            //}
            else
            {
                throw new MissingEventException($"Events collections doesn't contain an event with id {id}");
            }
        }
    }
}
