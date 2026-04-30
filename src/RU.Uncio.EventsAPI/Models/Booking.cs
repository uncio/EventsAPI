using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI.Models
{
    /// <summary>
    /// Booking statuses
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// Booking is on the way to be added
        /// </summary>
        Pending,
        /// <summary>
        /// Booking confirmed
        /// </summary>
        Confirmed,
        /// <summary>
        /// Booking rejected
        /// </summary>
        Rejected
    }
    /// <summary>
    /// Booking model
    /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        public Booking(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.Now;
        }
    }
}
