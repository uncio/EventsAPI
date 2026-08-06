namespace RU.Uncio.BookingService.Domain.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class MissingBookingException : ArgumentException
    {
        /// <summary>
        /// 
        /// </summary>
        public MissingBookingException()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MissingBookingException(string? message) : base(message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public MissingBookingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        public MissingBookingException(string? message, string? paramName) : base(message, paramName)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        /// <param name="innerException"></param>
        public MissingBookingException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
