namespace RU.Uncio.Domain.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class NoAvailableSeatsException : InvalidOperationException
    {
        /// <summary>
        /// 
        /// </summary>
        public NoAvailableSeatsException()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public NoAvailableSeatsException(string? message) : base(message)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public NoAvailableSeatsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
