using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected
    }
    public class Booking
    {
        /// <summary>
        /// Booking ID
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Event ID
        /// </summary>
        [Required]
        public Guid EventId { get; private set; }

        private BookingStatus status;
        /// <summary>
        /// Booking Status
        /// </summary>
        public BookingStatus Status
        {
            get { return status; }
            set
            {
                var changed = status != value;
                if (changed)
                {
                    status = value;
                    ProcessedAt = DateTime.Now;
                }
            }
        }
        /// <summary>
        /// Booking creation time
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Booking processed at
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        public Booking(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.Now;
        }
    }
}
