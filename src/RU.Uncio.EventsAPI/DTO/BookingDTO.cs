using RU.Uncio.EventsAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI.DTO
{
    public class BookingDTO
    {
        /// <summary>
        /// Booking ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Event ID
        /// </summary>
        public Guid EventId { get; set; }
        /// <summary>
        /// Booking Status
        /// </summary>
        public string Status { get; set; }
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
