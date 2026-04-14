using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Interfaces
{
    /// <summary>
    /// Events manipulation service interface
    /// </summary>
    public interface IEventsService
    {
        /// <summary>
        /// Gets all events from collection
        /// </summary>
        /// <returns>Collection of events</returns>
        List<Event> GetEvents();
        /// <summary>
        /// Gets an event from collection by ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <returns>Event instance</returns>
        Event GetEvent(Guid id);
        /// <summary>
        /// Adds an event to collection
        /// </summary>
        /// <param name="ev">Event to add</param>
        void AddEvent(Event ev);
        /// <summary>
        /// Replaces an event in collection by event ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="ev">Event to replace</param>
        void ReplaceEvent(Guid id, Event ev);
        /// <summary>
        /// Deletes an event from collection by event ID
        /// </summary>
        /// <param name="id"></param>
        void RemoveEvent(Guid id);
    }
}
