using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Repositories
{
    /// <summary>
    /// Concrete in memory events repository
    /// </summary>
    public class InMemoryEventRepository : IEventRepository
    {
        /// <summary>
        /// Collection of events
        /// </summary>
        public static Dictionary<Guid, Event> Events { get; private set; } = [];

        /// <summary>
        /// Adds an event to collection
        /// </summary>
        /// <param name="ev">Event to add</param>
        public void AddEvent(Event ev)
        {
            Events.Add(ev.Id, ev);
        }

        /// <summary>
        /// Gets all events from collection
        /// </summary>
        /// <returns>Collection of events</returns>
        public Dictionary<Guid, Event> GetEvents()
        {
            return Events;
        }

        /// <summary>
        /// Deletes an event from collection by event ID
        /// </summary>
        /// <param name="id"></param>
        public void RemoveEvent(Guid id)
        {
            Events.Remove(id);
        }

        /// <summary>
        /// Updates an event in collection by event ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="ev">Event to update</param>
        public void UpdateEvent(Guid id, Event ev)
        {
            Events[id].Title = ev.Title;
            Events[id].Description = ev.Description;
            Events[id].StartAt = ev.StartAt;
            Events[id].EndAt = ev.EndAt;
        }
    }
}
