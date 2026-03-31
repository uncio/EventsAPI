using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI.DTO
{
    /// <summary>
    /// Data Transfer Object of Event class
    /// </summary>
    public class EventDTO
    {
        /// <summary>
        /// Event ID
        /// </summary>
        [Required]
        public Guid Id { get; set; }
        /// <summary>
        /// Event title
        /// </summary>
        [Required]
        public string Title { get; set; }
        /// <summary>
        /// Event description (optional)
        /// </summary>
        public string Description { get; set; } = "";
        /// <summary>
        /// Event starts at
        /// </summary>
        [Required]
        [DateGreaterThanAttribute("EndAt")]
        public DateTime StartAt { get; set; }
        /// <summary>
        /// Event ends at
        /// </summary>
        [Required]
        public DateTime EndAt { get; set; }
    }
}
