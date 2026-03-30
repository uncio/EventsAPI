using Microsoft.AspNetCore.Mvc;
using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Services
{
    public class EventsService : IEventsService
    {
        public static Dictionary<Guid, Event> Events { get; private set; } = [];

        public List<Event> GetEvents()
        {
            return Events.Values.ToList();
        }

        public Event GetEvent(Guid id)
        {
            if(Events.TryGetValue(id, out Event ev))
                return ev;

            return null;
        }

        public void AddEvent(Event ev)
        {
            if(!Events.ContainsKey(ev.Id))
                Events[ev.Id] = ev;
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void ReplaceEvent(Guid id, Event ev)
        {
            if (Events.TryGetValue(id, out Event existingEvent))
            {
                Events[id] = ev;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void RemoveEvent(Guid id)
        {
            if (Events.TryGetValue(id, out _))
            {
                Events.Remove(id);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
