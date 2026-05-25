namespace RU.Uncio.EventsAPI.Models
{
    /// <summary>
    /// Event model
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Event ID
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Event title
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// Event description (optional)
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Event starts at
        /// </summary>
        public DateTime StartAt { get; set; }
        /// <summary>
        /// Event ends at
        /// </summary>
        public DateTime EndAt { get; set; }

        /// <summary>
        /// Total amount of seats
        /// </summary>
        public int TotalSeats { get; set; }
        /// <summary>
        /// Available amount of seats at current moment
        /// </summary>
        public int AvailableSeats { get; set; }
        /// <summary>
        /// Bookings for the event
        /// </summary>
        public List<Booking> Bookings { get; set; } = new();

        /// <summary>
        /// For deserialzer
        /// </summary>
        public Event() { }

        /// <summary>
        /// Event base constructor
        /// </summary>
        /// <param name="title"></param>
        /// <param name="starts"></param>
        /// <param name="ends"></param>
        /// <param name="total"></param>
        public Event(string title, DateTime starts, DateTime ends, int total)
        {
            Id = Guid.NewGuid();
            Title = title;
            StartAt = starts;
            EndAt = ends;
            TotalSeats = total;
            AvailableSeats = TotalSeats;
        }

        internal void UpdateWith(Event ev)
        {
            Title = ev.Title;
            Description = ev.Description;
            StartAt = ev.StartAt;
            EndAt = ev.EndAt;
        }

        internal bool TryReserveSeats(int count = 1)
        {
            if(AvailableSeats >= count)
            {
                AvailableSeats -= count;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// releases seats after booking cancelation or rejecting
        /// </summary>
        /// <param name="count"></param>
        public void ReleaseSeats(int count = 1)
        {
            AvailableSeats += count;
            AvailableSeats = AvailableSeats <= TotalSeats ? AvailableSeats : TotalSeats;
        }


    }
}
