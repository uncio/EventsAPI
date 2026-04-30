using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI.DTO
{
    /// <summary>
    /// Data Transfer Object for Event model
    /// </summary>
    public record EventDTO
    {
        /// <summary>
        /// Event ID
        /// </summary>
        public Guid Id { get; internal set; }
        /// <summary>
        /// Event title
        /// </summary>
        [Required]
        public required string Title { get; set; }
        /// <summary>
        /// Event description (optional)
        /// </summary>
        public string? Description { get; set; } = "";
        /// <summary>
        /// Event starts at
        /// </summary>
        [Required]
        [DateGreaterThan("EndAt")]
        public DateTime StartAt { get; set; }
        /// <summary>
        /// Event ends at
        /// </summary>
        [Required]
        public DateTime EndAt { get; set; }        
    }
}
