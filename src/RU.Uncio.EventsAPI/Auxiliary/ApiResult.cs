using System.Net;

namespace RU.Uncio.EventsAPI
{
    /// <summary>
    /// Concrete result class with returning content
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> : ApiBaseResult
    {
        /// <summary>
        /// Returning content
        /// </summary>
        public required T Data { get; set; }
    }

    /// <summary>
    /// Conrete result class - NoContent
    /// </summary>
    public class ApiResult : ApiBaseResult { }

    /// <summary>
    /// Base result class with base properties
    /// </summary>
    public class ApiBaseResult
    {
        /// <summary>
        /// Success result flag
        /// </summary>
        public required bool Success { get; set; }
        /// <summary>
        /// Returning HTTP-code
        /// </summary>
        public required HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Date and time of responce
        /// </summary>
        public DateTime DateTime { get; private set; } = DateTime.UtcNow;
        /// <summary>
        /// Custom message with auxiliary information and possible errors
        /// </summary>
        public required string Message { get; set; }
    }
}
