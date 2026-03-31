namespace RU.Uncio.EventsAPI.Models
{
    /// <summary>
    /// Class containing event properties
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Event ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Event title
        /// </summary>
        public string Title { get; set; }
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

        public Event(Guid id, string title, DateTime starts, DateTime ends)
        {
            Id = id;
            Title = title;
            StartAt = starts;
            EndAt = ends;

            #region alternative timeline validation
            //if(!EndAt.IsStrictlyGreaterThan(StartAt))
            //{
            //    throw new ArgumentException("Event end time is to be later than start time");
            //}
            #endregion
        }
    }
}
