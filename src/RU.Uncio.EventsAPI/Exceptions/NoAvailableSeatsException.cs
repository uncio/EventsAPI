using System.Runtime.Serialization;

namespace RU.Uncio.EventsAPI.Exceptions
{
    public class NoAvailableSeatsException : InvalidOperationException
    {
        public NoAvailableSeatsException()
        {
        }

        public NoAvailableSeatsException(string? message) : base(message)
        {
        }

        public NoAvailableSeatsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
