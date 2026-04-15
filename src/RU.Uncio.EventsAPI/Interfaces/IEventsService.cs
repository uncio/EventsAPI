using Microsoft.AspNetCore.Mvc;
using RU.Uncio.EventsAPI.DTO;
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
        List<Event> GetEvents(string? title, DateTime? from, DateTime? to);
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
        /// Updates an event in collection by event ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="ev">Event to update</param>
        void UpdateEvent(Guid id, Event ev);
        /// <summary>
        /// Deletes an event from collection by event ID
        /// </summary>
        /// <param name="id"></param>
        void RemoveEvent(Guid id);

        /// <summary>
        /// Returns paginated events
        /// </summary>
        /// <param name="filtered">events after filtering</param>
        /// <param name="page">page number</param>
        /// <param name="pageSize">items number per page</param>
        /// <returns></returns>
        PaginatedResultDTO<EventDTO> GetPaginatedEvents(IEnumerable<EventDTO> filtered, int page, int pageSize);
    }
}
