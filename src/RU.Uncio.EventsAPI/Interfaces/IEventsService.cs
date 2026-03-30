using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Interfaces
{
    public interface IEventsService
    {
        List<Event> GetEvents();
        Event GetEvent(Guid id);
        void AddEvent(Event ev);
        void ReplaceEvent(Guid id, Event ev);
        void RemoveEvent(Guid id);
    }
}
