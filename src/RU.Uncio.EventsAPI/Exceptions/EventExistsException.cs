using System.Runtime.Serialization;

namespace RU.Uncio.EventsAPI.Exceptions
{
    public class EventExistsException : ArgumentException
    {
        public EventExistsException()
        {
        }

        public EventExistsException(string? message) : base(message)
        {
        }

        public EventExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public EventExistsException(string? message, string? paramName) : base(message, paramName)
        {
        }

        public EventExistsException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
