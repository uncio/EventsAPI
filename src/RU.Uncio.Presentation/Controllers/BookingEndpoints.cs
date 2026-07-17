using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RU.Uncio.Application.Auxiliary;
using RU.Uncio.Application.DTO;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.EventsAPI;
using System.Net;
using System.Security.Claims;

namespace RU.Uncio.Presentation.Controllers
{
    /// <summary>
    /// Mapping extensions
    /// </summary>
    public static class BookingEndpoints
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder endpoints, ILogger logger)
        {
            endpoints.MapPost("Events/{eventId}/book", [Authorize] async ([FromRoute] Guid eventId, IBookingService service, ClaimsPrincipal user, CancellationToken token) =>
            {
                try
                {
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null)
                    {
                        return Results.BadRequest(new ApiResult
                        {
                            Success = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Message = $"User identifier is not found"
                        });
                    }

                    var userId = new Guid(userIdClaim.Value);

                    var result = await service.CreateBookingAsync(userId, eventId, token);

                    if (result != null)
                    {
                        var booking = result.MapToDto();
                        logger.LogInformation("Booking processed");
                        return Results.Accepted(uri: $"/bookings/{booking.Id}", value: new ApiResult<BookingDTO>
                        {
                            Data = booking,
                            Success = true,
                            StatusCode = HttpStatusCode.Accepted,
                            Message = $"Adding booking for event with ID {eventId} in collection"
                        });
                    }
                    else
                    {
                        return Results.BadRequest(new ApiResult
                        {
                            Success = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Message = $"Failed to create booking for user {userId} for event {eventId}"
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Client Closed Request");
                    return Results.StatusCode(499); //Client Closed Request
                }
            });

            endpoints.MapGet("/bookings/{id}", [Authorize] async ([FromRoute] Guid id, IBookingService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.GetBookingByIdAsync(id, token);
                    if (result != null)
                    {
                        var booking = result.MapToDto();
                        return Results.Ok(value: new ApiResult<BookingDTO>
                        {
                            Data = booking,
                            Success = true,
                            StatusCode = HttpStatusCode.OK,
                            Message = $"Getting booking with ID {id} from collection"
                        });
                    }
                    else
                    {
                        return Results.NotFound(new ApiResult
                        {
                            Success = false,
                            StatusCode = HttpStatusCode.NotFound,
                            Message = $"Booking with ID {id} is not found in the collection"
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Client Closed Request");
                    return Results.StatusCode(499); //Client Closed Request
                }
            });

            endpoints.MapDelete("/bookings/{id}", [Authorize] async ([FromRoute] Guid id, IBookingService service, ClaimsPrincipal user, CancellationToken token) =>
            {
                try
                {
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null)
                    {
                        return Results.BadRequest(new ApiResult
                        {
                            Success = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Message = $"User identifier is not found"
                        });
                    }

                    var userId = new Guid(userIdClaim.Value);

                    await service.CancelBookingByIdAsync(userId, id, token);
                    return Results.NoContent();
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Client Closed Request");
                    return Results.StatusCode(499); //Client Closed Request
                }
            });

            return endpoints;
        }
    }
}
