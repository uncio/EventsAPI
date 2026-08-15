using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Application.Auxiliary;
using RU.Uncio.EventService.Application.DTO;
using RU.Uncio.EventService.Domain.Models;

namespace RU.Uncio.EventService.Presentation.Controllers
{
    /// <summary>
    /// Events controller
    /// </summary>
    /// <param name="eventsService"></param>
    /// <param name="logger"></param>
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class EventsController(IEventsService eventsService, ILogger<EventsController> logger) : ControllerBase
    {
        /// <summary>
        /// Returns paginated events from collection
        /// </summary>
        /// <response code="200">JSON-schema of ApiResult is returned with events and detailed responce
        /// and HTTP status-code 200 Ok in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResult<PaginatedResultDTO<EventDTO>>>> GetEventsAsync(CancellationToken token,
                                                                    [FromQuery] string? title = null,
                                                                    [FromQuery] DateTime? from = null,
                                                                    [FromQuery] DateTime? to = null,
                                                                    [FromQuery] int page = 1,
                                                                    [FromQuery] int pageSize = 10)
        {
            var events = await eventsService.GetEventsAsync(token, title, from, to);                
            var paginatedEvents = eventsService.GetPaginatedEvents(events, page, pageSize, out int totalPages)
                .Select(ev => ev.MapToDto());

            var result = new PaginatedResultDTO<EventDTO>
                (
                    paginatedEvents.ToList(),
                    paginatedEvents.Count(),
                    page,
                    totalPages,
                    events.Count
                );

            return Ok(new ApiResult<PaginatedResultDTO<EventDTO>>
            {
                Data = result,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Gettin paginated events from collection"
            });
        }

        /// <summary>
        /// Returns top 10 events with most bookings
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet("/top")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResult<List<EventDTO>>>> GetTop10EventsAsync(CancellationToken token)
        {
            var top10Events = await eventsService.GetTop10EventsAsync(token);
            var result = top10Events.Select(ev => ev.MapToDto()).ToList();

             return Ok(new ApiResult<List<EventDTO>>
            {
                Data = result,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Gettin top 10 events from collection"
            });
        }

        /// <summary>
        /// Returns an event by ID from collection
        /// </summary>
        /// <param name="id">Id parameter to get an event</param>
        /// <param name="token"></param>
        /// <response code="200">JSON-schema of ApiBaseResult is returned with found event and detailed responce
        /// and HTTP status-code 200 Ok in case of success</response>
        [ProducesResponseType(typeof(ApiBaseResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet("{id:Guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiBaseResult>> GetEventById([FromRoute] Guid id, CancellationToken token)
        {
            var eventById = await eventsService.GetEventAsync(id, token);

            if (eventById != null)
            {
                var result = eventById.MapToDto();

                return Ok(new ApiResult<EventDTO>
                {
                    Data = result,
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = $"Getting event with ID {id} from collection"
                });
            }
            else
            {
                logger.LogError($"Event with ID {id} is not found in the collection");
                return NotFound(new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Event with ID {id} is not found in the collection"
                });
            }
        }

        /// <summary>
        /// Adds an event from request body to collection
        /// </summary>
        /// <param name="ev">Event from request body to add</param>
        /// <param name="token"></param>
        /// <response code="201">JSON-schema is returned of ApiResult with detailed responce
        /// and HTTP status-code 201 Created in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult>> CreateEvent([FromBody] EventDTO ev, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"Validation failed: {String.Join(";",
                    ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
                throw new ValidationException($"Validation failed: {String.Join(";",
                    ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
            }

            var newEvent = new Event(ev.Title ?? "", ev.StartAt, ev.EndAt, ev.TotalSeats) { Description = ev.Description };
            await eventsService.AddEventAsync(newEvent, token);

            return CreatedAtAction(nameof(CreateEvent), new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.Created,
                Message = "Adding the event to the collection"
            });
        }

        /// <summary>
        /// Updates an event from request body by ID in collection
        /// </summary>
        /// <param name="id">Id parameter to update an event</param>
        /// <param name="ev">Event from request body to update</param>
        /// <param name="token"></param>
        /// <response code="204">JSON-schema is returned of ApiResult with detailed responce
        /// and HTTP status-code 204 NoContent in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
        [Consumes("application/json")]
        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateEvent([FromRoute] Guid id, [FromBody] EventDTO ev, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"Validation failed: {String.Join(";",
                    ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
                throw new ValidationException($"Validation failed: {String.Join(";",
                    ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
            }

            var newEvent = new Event(ev.Title ?? "", ev.StartAt, ev.EndAt, ev.TotalSeats) { Description = ev.Description };
            await eventsService.UpdateEventAsync(id, newEvent, token);
            return NoContent();
        }

        /// <summary>
        /// Deletes an event by ID from collection
        /// </summary>
        /// <param name="id">Id parameter of the event to delete</param>
        /// <param name="token"></param>
        /// <response code="204">JSON-schema is returned of ApiResult with detailed responce
        /// and HTTP status-code 204 NoContent in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteEvent([FromRoute] Guid id, CancellationToken token)
        {
            await eventsService.RemoveEventAsync(id, token);
            return NoContent();
        }
    }
}
