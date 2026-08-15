using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Domain.Models;
using RU.Uncio.EventService.Infrastructure.DataAccess;
using StackExchange.Redis;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;

namespace RU.Uncio.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete events repository
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext db;
        private readonly IDatabase redis;
        private readonly ILogger<EventRepository> logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dB"></param>
        public EventRepository(AppDbContext dB, IConnectionMultiplexer red, ILogger<EventRepository> log) { db = dB; redis = red.GetDatabase(); logger = log; }

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
        /// Returns event by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Event> GetEventAsync(Guid id, CancellationToken token)
        {
            var cacheKey = $"event:{id}";

            try
            {
                var cached = await redis.StringGetAsync(cacheKey);
                if (cached.HasValue)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    return JsonSerializer.Deserialize<Event>((string)cached!, (JsonSerializerOptions)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
            }


            var ev = await db.Events.FindAsync(id);
            if (ev == null) return null;

            try
            {
                await redis.StringSetAsync(
                    cacheKey,
                    JsonSerializer.Serialize(ev),
                    TimeSpan.FromMinutes(5)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
            }

            return ev;
        }

        /// <summary>
        /// Gets all events from DB
        /// </summary>
        /// <returns>Collection of events</returns>
        public async Task<Dictionary<Guid, Event>> GetEventsAsync(CancellationToken token) => await db.Events.ToDictionaryAsync(ev => ev.Id, token);

        /// <summary>
        /// Get top 10 events
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<Event>> GetTop10EventsAsync(CancellationToken token)
        {
            var cacheKey = $"events:top10";

            try
            {
                var cached = await redis.StringGetAsync(cacheKey);
                if (cached.HasValue)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    return JsonSerializer.Deserialize<List<Event>>((string)cached!, (JsonSerializerOptions)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
            }

            var events = db.Events.ToList();
            events.Sort((ev1, ev2) =>
            {
                var sold1 = (ev1.TotalSeats - ev1.AvailableSeats) / ev1.TotalSeats;
                var sold2 = (ev2.TotalSeats - ev2.AvailableSeats) / ev2.TotalSeats;

                return sold2.CompareTo(sold1);
            });

            try
            {
                await redis.StringSetAsync(
                    cacheKey,
                    JsonSerializer.Serialize(events.Take(10)),
                    TimeSpan.FromMinutes(15)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
            }

            return events;
        }

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

            string cacheKey = $"event:{id}";
            await redis.KeyDeleteAsync(cacheKey);
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

            string cacheKey = $"event:{ev.Id}";
            try
            {
                await redis.StringSetAsync(
                    cacheKey,
                    JsonSerializer.Serialize(ev),
                    TimeSpan.FromMinutes(15)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
            }
        }
    }
}
