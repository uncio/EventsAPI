namespace RU.Uncio.BookingService.Domain.Models
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
        Rejected,
        /// <summary>
        /// Booking cancelled
        /// </summary>
        Cancelled
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
        /// User id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Event ID
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Booking Status
        /// </summary>
        public BookingStatus Status { get; set; }
        /// <summary>
        /// Booking creation time
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Booking processed at
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// For serializer
        /// </summary>
        public Booking() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventId"></param>
        public Booking(Guid userId, Guid eventId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            EventId = eventId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.Now.ToUniversalTime();
        }
        /// <summary>
        /// Confirm status setter
        /// </summary>
        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Reject status setter
        /// </summary>
        public void Reject()
        {
            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Cancell status setter
        /// </summary>
        public void Cancell()
        {
            if (Status != BookingStatus.Cancelled)
            {
                Status = BookingStatus.Cancelled;
                ProcessedAt = DateTime.Now.ToUniversalTime();
            }
        }
    }
}
