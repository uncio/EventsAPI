using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.Application.DTO
{
    /// <summary>
    /// Data transfer object for Booking model
    /// </summary>
    public class BookingDTO
    {
        /// <summary>
        /// Booking ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Event ID
        /// </summary>
        ///         
        [Required]
        public Guid EventId { get; set; }
        /// <summary>
        /// Booking Status
        /// </summary>
        public required string Status { get; set; }
        /// <summary>
        /// Booking creation time
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Booking processed at
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
    }
}
