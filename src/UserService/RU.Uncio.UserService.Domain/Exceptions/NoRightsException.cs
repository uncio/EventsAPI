namespace RU.Uncio.UserService.Domain.Exceptions
{
    /// <summary>
    /// Exception in case of user doesn;t have rights for the request
    /// </summary>
    public class NoRightsException : ArgumentException
    {
        /// <summary>
        /// 
        /// </summary>
        public NoRightsException()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public NoRightsException(string? message) : base(message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public NoRightsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        public NoRightsException(string? message, string? paramName) : base(message, paramName)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        /// <param name="innerException"></param>
        public NoRightsException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
