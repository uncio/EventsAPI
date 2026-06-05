using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Interfaces
{
    /// <summary>
    /// Events Repository wrapper
    /// </summary>
    public interface IEventRepository
    {
        /// <summary>
        /// Gets all events from collection
        /// </summary>
        /// <returns>Collection of events</returns>
        Task<Dictionary<Guid, Event>> GetEventsAsync(CancellationToken token);
        /// <summary>
        /// Adds an event to collection
        /// </summary>
        /// <param name="ev">Event to add</param>
        /// <param name="token"></param>
        Task AddEventAsync(Event ev, CancellationToken token);
        /// <summary>
        /// Updates an event in collection by event ID
        /// </summary>
        /// <param name="ev">Event to update</param>
        Task UpdateEventAsync(Event ev, CancellationToken token);
        /// <summary>
        /// Deletes an event from collection by event ID
        /// </summary>
        /// <param name="id"></param>
        Task RemoveEventAsync(Guid id, CancellationToken token);
    }
}
