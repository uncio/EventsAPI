using System.Runtime.Serialization;

namespace RU.Uncio.EventsAPI.Exceptions
{
    public class MissingEventException : ArgumentException
    {
        public MissingEventException()
        {
        }

        public MissingEventException(string? message) : base(message)
        {
        }

        public MissingEventException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public MissingEventException(string? message, string? paramName) : base(message, paramName)
        {
        }

        public MissingEventException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
