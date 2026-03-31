using System.Net;

namespace RU.Uncio.EventsAPI.Helpers
{
    public class ApiResult<T> : ApiBaseResult
    {
        public required T Data { get; set; }
    }

    public class ApiResult : ApiBaseResult { }

    public class ApiBaseResult
    {
        public required bool Success { get; set; }
        public required HttpStatusCode StatusCode { get; set; }
        public DateTime DateTime { get; private set; } = DateTime.UtcNow;
        public required string Message { get; set; }
    }
}
