using System.Runtime.Serialization;

namespace RU.Uncio.EventsAPI.Exceptions
{
    /// <summary>
    /// Event with given ID doesn;t exist within the collection
    /// </summary>
    public class MissingEventException : ArgumentException
    {
        /// <summary>
        /// 
        /// </summary>
        public MissingEventException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MissingEventException(string? message) : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public MissingEventException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        public MissingEventException(string? message, string? paramName) : base(message, paramName)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        /// <param name="innerException"></param>
        public MissingEventException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
