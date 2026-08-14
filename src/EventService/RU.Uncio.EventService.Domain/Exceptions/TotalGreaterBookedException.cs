namespace RU.Uncio.EventService.Domain.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class TotalGreaterBookedException : ArgumentException
    {
        /// <summary>
        /// 
        /// </summary>
        public TotalGreaterBookedException()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public TotalGreaterBookedException(string? message) : base(message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public TotalGreaterBookedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        public TotalGreaterBookedException(string? message, string? paramName) : base(message, paramName)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="paramName"></param>
        /// <param name="innerException"></param>
        public TotalGreaterBookedException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
