using System.Runtime.Serialization;

namespace RU.Uncio.EventsAPI.Exceptions
{
    public class BookingNotFoundException : KeyNotFoundException
    {
        public BookingNotFoundException()
        {
        }

        public BookingNotFoundException(string? message) : base(message)
        {
        }

        public BookingNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
