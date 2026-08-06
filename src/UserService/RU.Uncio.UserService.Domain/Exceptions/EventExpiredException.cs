namespace RU.Uncio.UserService.Domain.Exceptions
{
    /// <summary>
    /// Exception in case of try to book a started event
    /// </summary>
    public class EventExpiredException : ArgumentException
    {
        /// <summary>
        /// 
        /// </summary>
        public EventExpiredException()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public EventExpiredException(string? message) : base(message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public EventExpiredException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        public EventExpiredException(string? message, string? paramName) : base(message, paramName)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        /// <param name="innerException"></param>
        public EventExpiredException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
