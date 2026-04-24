using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Interfaces
{
    public interface IBookingRepository
    {
        /// <summary>
        /// Gets all events from collection
        /// </summary>
        /// <returns>Collection of events</returns>
        Dictionary<Guid, Booking> GetBookings();
        /// <summary>
        /// Adds an event to collection
        /// </summary>
        /// <param name="ev">Event to add</param>
        void AddBooking(Booking book);
        /// <summary>
        /// Updates an event in collection by event ID
        /// </summary>
        /// <param name="id">ID parameter of event</param>
        /// <param name="ev">Event to update</param>
        void UpdateBooking(Guid id, BookingStatus status);
        /// <summary>
        /// Deletes an event from collection by event ID
        /// </summary>
        /// <param name="id"></param>
        void RemoveBooking(Guid id);
    }
}
