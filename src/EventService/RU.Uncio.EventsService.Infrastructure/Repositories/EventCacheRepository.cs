using Microsoft.Extensions.Logging;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Domain.Models;
using RU.Uncio.EventService.Infrastructure.DataAccess;
using RU.Uncio.Infrastructure.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RU.Uncio.EventsService.Infrastructure.Repositories
{
    internal class EventCacheRepository : IEventCacheRepository
    {
        private readonly IDatabase redis;
        private readonly ILogger<EventRepository> logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="red"></param>
        /// <param name="log"></param>
        public EventCacheRepository( IConnectionMultiplexer red, ILogger<EventRepository> log) { redis = red.GetDatabase(); logger = log; }
        public async Task<Event?> GetByIdAsync(Guid id)
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
            }            

            return null;
        }

        public async Task<List<Event>> GetTop10Async()
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

            return null;
        }

        public async Task RemoveEventAsync(Guid id)
        {
            string cacheKey = $"event:{id}";
            try
            {
                await redis.KeyDeleteAsync(cacheKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to Redis");
            }
        }

        public async Task SetAsync(Event ev)
        {
            var cacheKey = $"event:{ev.Id}";

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
        }

        public async Task SetTop10Async(List<Event> events)
        {
            var cacheKey = $"events:top10";

            try
            {
                await redis.StringSetAsync(
                    cacheKey,
                    JsonSerializer.Serialize(events),
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
