using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Repositories
{
    public class InMemoryEventRepository : IEventRepository
    {
        /// <summary>
        /// Collection of events
        /// </summary>
        public static Dictionary<Guid, Event> Events { get; private set; } = [];
        public void AddEvent(Event ev)
        {
            Events.Add(ev.Id, ev);
        }

        public Dictionary<Guid, Event> GetEvents()
        {
            return Events;
        }

        public void RemoveEvent(Guid id)
        {
            Events.Remove(id);
        }

        public void UpdateEvent(Guid id, Event ev)
        {
            Events[id].Title = ev.Title;
            Events[id].Description = ev.Description;
            Events[id].StartAt = ev.StartAt;
            Events[id].EndAt = ev.EndAt;
        }
    }
}
