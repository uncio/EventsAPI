using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
{
    /// <summary>
    /// Service to manipulate with events collection
    /// </summary>
    public class EventsService : IEventsService
    {
        /// <summary>
        /// Collection of events
        /// </summary>
        public static Dictionary<Guid, Event> Events { get; private set; } = [];

        /// <summary>
        /// Gets all events from collection
        /// </summary>
        /// <returns></returns>
        public List<Event> GetEvents()
        {
            return Events.Values.ToList();
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
                throw new ArgumentException($"Event with ID {ev.Id} already exists in the collection");
            }
        }

        /// <summary>
        /// Replaces an event i collection by ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="ev">Event to replace</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void ReplaceEvent(Guid id, Event ev)
        {
            if (Events.TryGetValue(id, out Event existingEvent))
            {
                Events[id] = ev;
            }
            else
            {
                throw new IndexOutOfRangeException();
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
                throw new IndexOutOfRangeException();
            }
        }
    }
}
