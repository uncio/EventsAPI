namespace RU.Uncio.EventService.Domain.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class MissingUserException : ArgumentException
    {
        /// <summary>
        /// 
        /// </summary>
        public MissingUserException()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MissingUserException(string? message) : base(message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public MissingUserException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        public MissingUserException(string? message, string? paramName) : base(message, paramName)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        /// <param name="innerException"></param>
        public MissingUserException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
