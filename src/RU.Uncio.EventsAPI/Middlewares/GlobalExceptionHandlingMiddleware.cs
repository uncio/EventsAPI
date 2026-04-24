using Microsoft.AspNetCore.Mvc;
using RU.Uncio.EventsAPI.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace RU.Uncio.EventsAPI.Middlewares
{
    /// <summary>
    /// Final exception handler
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleException(httpContext, ex);
            }
        }

        private async Task HandleException(HttpContext httpContext, Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception. Method={Method}, Path={Path}, RequestId={RequestId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.Headers["x-request-id"]);

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            var statusCode = MapIntStatusCode(ex);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var error = new ApiResult
            {
                Success = false,
                StatusCode = MapStatusCode(ex),
                Message = ex.Message
            };

            await httpContext.Response.WriteAsJsonAsync(error);
        }

        private static int MapIntStatusCode(Exception ex)
            => ex switch
            {
                ValidationException ve => StatusCodes.Status400BadRequest,
                EventExistsException eee => StatusCodes.Status400BadRequest,
                MissingEventException mee => StatusCodes.Status404NotFound,
                BookingNotFoundException mee => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

        private static HttpStatusCode MapStatusCode(Exception ex)
            => ex switch
            {
                ValidationException ve => HttpStatusCode.BadRequest,
                EventExistsException eee => HttpStatusCode.BadRequest,
                MissingEventException mee => HttpStatusCode.NotFound,
                BookingNotFoundException mee => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };
    }
}
