using Microsoft.EntityFrameworkCore;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.Domain.Models;
using RU.Uncio.Infrastructure.DataAccess;

namespace RU.Uncio.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete in memory events repository
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext db;
        public EventRepository(AppDbContext dB) { db = dB; }

        /// <summary>
        /// Adds an event to DB
        /// </summary>
        /// <param name="ev">Event to add</param>
        public async Task AddEventAsync(Event ev, CancellationToken token)
        {
            await db.Events.AddAsync(ev, token);
            await db.SaveChangesAsync(token);
        }

        /// <summary>
        /// Gets all events from DB
        /// </summary>
        /// <returns>Collection of events</returns>
        public async Task<Dictionary<Guid, Event>> GetEventsAsync(CancellationToken token) => await db.Events.ToDictionaryAsync(ev => ev.Id, token);

        /// <summary>
        /// Deletes an event from collection by event ID
        /// </summary>
        /// <param name="id"></param>
        public async Task RemoveEventAsync(Guid id, CancellationToken token)
        {
            var @event = new Event(id);
            db.Events.Remove(@event);
            await db.SaveChangesAsync(token);
        }

        /// <summary>
        /// Updates an event in collection by event ID
        /// </summary>
        /// <param name="ev">Event to update</param>
        public async Task UpdateEventAsync(Event ev, CancellationToken token)
        {
            db.Events.Update(ev);
            await db.SaveChangesAsync(token);
        }
    }
}
