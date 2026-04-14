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
        public Guid Id { get; private set; }
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

        /// <summary>
        /// Event base constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="starts"></param>
        /// <param name="ends"></param>
        public Event(string title, DateTime starts, DateTime ends)
        {
            Id = Guid.NewGuid();
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

        internal void UpdateWith(Event ev)
        {
            Title = ev.Title;
            Description = ev.Description;
            StartAt = ev.StartAt;
            EndAt = ev.EndAt;
        }
    }
}
