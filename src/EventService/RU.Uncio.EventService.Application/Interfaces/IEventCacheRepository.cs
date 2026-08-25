using RU.Uncio.EventService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RU.Uncio.EventService.Application.Interfaces
{
    public interface IEventCacheRepository
    {
        Task<Event?> GetByIdAsync(Guid id);
        Task SetAsync(Event ev);
        Task<List<Event>> GetTop10Async();
        Task SetTop10Async(List<Event> events);
        Task RemoveEventAsync(Guid id);
    }
}
