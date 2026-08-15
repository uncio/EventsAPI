
using RU.Uncio.EventService.Domain.Models;

namespace RU.Uncio.EventService.Application.Interfaces
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
        Task<List<Event>> GetEventsAsync(CancellationToken token, string? title, DateTime? from, DateTime? to);
        /// <summary>
        /// Gets top 10 events from collection
        /// </summary>
        /// <returns>Collection of events</returns>
        Task<List<Event>> GetTop10EventsAsync(CancellationToken token);
        /// <summary>
        /// Gets an event from collection by ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="token">cancellation token</param>
        /// <returns>Event instance</returns>
        Task<Event> GetEventAsync(Guid id, CancellationToken token);
        /// <summary>
        /// Adds an event to collection
        /// </summary>
        /// <param name="ev">Event to add</param>
        /// <param name="token">cancellation token</param>
        Task AddEventAsync(Event ev, CancellationToken token);
        /// <summary>
        /// Updates an event in collection by event ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="ev">Event to update</param>
        /// <param name="token">cancellation token</param>
        Task UpdateEventAsync(Guid id, Event ev, CancellationToken token);
        /// <summary>
        /// Deletes an event from collection by event ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token">cancellation token</param>
        Task RemoveEventAsync(Guid id, CancellationToken token);
        /// <summary>
        /// Returns paginated events
        /// </summary>
        /// <param name="filtered">events after filtering</param>
        /// <param name="page">page number</param>
        /// <param name="pageSize">items number per page</param>
        /// <param name="totalPages"></param>
        /// <returns>calculated amount of pages</returns>
        List<Event> GetPaginatedEvents(IEnumerable<Event> filtered, int page, int pageSize, out int totalPages);
    }
}
