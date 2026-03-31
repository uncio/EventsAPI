using RU.Uncio.EventsAPI.Helpers;
using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public Event(Guid id, string title, DateTime starts, DateTime ends)
        {
            Id = id;
            Title = title;
            StartAt = starts;
            EndAt = ends;

            if(!EndAt.IsStrictlyGreaterThan(StartAt))
            {
                throw new ArgumentException("Event end time is to be later than start time");
            }
        }
    }
}
