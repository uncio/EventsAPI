using RU.Uncio.EventsAPI.Helpers;
using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI.DTO
{
    public class EventDTO
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; } = "";
        [Required]
        //[DateGreaterThanAttribute("EndAt")]
        public DateTime StartAt { get; set; }
        [Required]
        public DateTime EndAt { get; set; }
    }
}
