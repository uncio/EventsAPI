using RU.Uncio.EventService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RU.Uncio.EventService.Application.Interfaces
{
    public interface IEventCacheRepository
    {
        Task<Event?> GetByIdAsync(int id);
        Task<List<Event>> GetTop10Async();
    }
}
