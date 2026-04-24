using System.Runtime.Serialization;

namespace RU.Uncio.EventsAPI.Exceptions
{
    /// <summary>
    /// Event already exists within the collection
    /// </summary>
    public class EventExistsException : ArgumentException
    {
        /// <summary>
        /// 
        /// </summary>
        public EventExistsException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public EventExistsException(string? message) : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public EventExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        public EventExistsException(string? message, string? paramName) : base(message, paramName)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        /// <param name="innerException"></param>
        public EventExistsException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
