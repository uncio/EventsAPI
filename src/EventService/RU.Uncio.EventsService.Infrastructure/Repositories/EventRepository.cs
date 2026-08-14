using Microsoft.EntityFrameworkCore;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Domain.Models;
using RU.Uncio.EventService.Infrastructure.DataAccess;

namespace RU.Uncio.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete events repository
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext db;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dB"></param>
        public EventRepository(AppDbContext dB) { db = dB; }

        /// <summary>
        /// Adds an event to DB
        /// </summary>
        /// <param name="ev">Event to add</param>
        /// <param name="token">cancellation token</param>
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
        /// <param name="token">cancellation token</param>
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
        /// <param name="token">cancellation token</param>
        public async Task UpdateEventAsync(Event ev, CancellationToken token)
        {
            db.Events.Update(ev);
            await db.SaveChangesAsync(token);
        }
    }
}
