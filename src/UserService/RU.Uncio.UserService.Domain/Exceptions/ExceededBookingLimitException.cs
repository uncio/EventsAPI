namespace RU.Uncio.UserService.Domain.Exceptions
{
    /// <summary>
    /// Exception in case of try to book an event over a user bookings limit
    /// </summary>
    public class ExceededBookingLimitException : ArgumentException
    {
        /// <summary>
        /// 
        /// </summary>
        public ExceededBookingLimitException()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ExceededBookingLimitException(string? message) : base(message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ExceededBookingLimitException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        public ExceededBookingLimitException(string? message, string? paramName) : base(message, paramName)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        /// <param name="innerException"></param>
        public ExceededBookingLimitException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
