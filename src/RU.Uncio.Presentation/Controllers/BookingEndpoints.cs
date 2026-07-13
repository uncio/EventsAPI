using Microsoft.AspNetCore.Mvc;
using RU.Uncio.Application.DTO;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.EventsAPI;
using System.Net;
using RU.Uncio.Application.Auxiliary;

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
            endpoints.MapPost("Events/{id}/book", async ([FromRoute] Guid id, IBookingService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.CreateBookingAsync(id, token);

                    if (result != null)
                    {
                        var booking = result.MapToDto();
                        logger.LogInformation("Booking processed");
                        return Results.Accepted(uri: $"/bookings/{booking.Id}", value: new ApiResult<BookingDTO>
                        {
                            Data = booking,
                            Success = true,
                            StatusCode = HttpStatusCode.Accepted,
                            Message = $"Adding booking for event with ID {id} in collection"
                        });
                    }
                    else
                    {
                        return Results.BadRequest(new ApiResult
                        {
                            Success = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Message = $"Event with ID {id} is not found in the collection"
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Client Closed Request");
                    return Results.StatusCode(499); //Client Closed Request
                }
            });

            endpoints.MapGet("/bookings/{id}", async ([FromRoute] Guid id, IBookingService service, CancellationToken token) =>
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

            return endpoints;
        }
    }
}
