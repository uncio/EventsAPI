using Microsoft.AspNetCore.Mvc;
using RU.Uncio.EventsAPI.Auxiliary;
using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Exceptions;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using RU.Uncio.EventsAPI.Services;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace RU.Uncio.EventsAPI.Controllers
{
    /// <summary>
    /// Events controller
    /// </summary>
    /// <param name="eventsService"></param>
    /// <param name="logger"></param>
    [ApiController]
    [Route("[controller]")]
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
        public ActionResult<ApiResult<PaginatedResultDTO<EventDTO>>> GetEvents([FromQuery] string? title = null,
                                                                    [FromQuery] DateTime? from = null,
                                                                    [FromQuery] DateTime? to = null,
                                                                    [FromQuery] int page = 1,
                                                                    [FromQuery] int pageSize = 10)
        {
            var events = eventsService.GetEvents(title, from, to);                
            var paginatedEvents = eventsService.GetPaginatedEvents(events, page, pageSize, out int totalPages)
                .Select(ev => ev.MapToDto());

            var result = new PaginatedResultDTO<EventDTO>
                (
                    paginatedEvents.ToList(),
                    paginatedEvents.Count(),
                    page,
                    totalPages,
                    events.Count()
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
        /// Returns an event by ID from collection
        /// </summary>
        /// <param name="id">Id parameter to get an event</param>
        /// <response code="200">JSON-schema of ApiBaseResult is returned with found event and detailed responce
        /// and HTTP status-code 200 Ok in case of success</response>
        [ProducesResponseType(typeof(ApiBaseResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet("{id:Guid}")]
        public ActionResult<ApiBaseResult> GetEventById([FromRoute] Guid id)
        {
            var eventById = eventsService.GetEvent(id);

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
        /// <response code="201">JSON-schema is returned of ApiResult with detailed responce
        /// and HTTP status-code 201 Created in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [HttpPost]
        public ActionResult<ApiResult> CreateEvent([FromBody] EventDTO ev)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"Validation failed: {String.Join(";", ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
                throw new ValidationException($"Validation failed: {String.Join(";", ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
            }

            var newEvent = new Event(ev.Title ?? "", ev.StartAt, ev.EndAt) { Description = ev.Description };
            eventsService.AddEvent(newEvent);

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
        /// <response code="204">JSON-schema is returned of ApiResult with detailed responce
        /// and HTTP status-code 204 NoContent in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
        [Consumes("application/json")]
        [HttpPut("{id:Guid}")]
        public ActionResult UpdateEvent([FromRoute] Guid id, [FromBody] EventDTO ev)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError($"Validation failed: {String.Join(";", ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
                throw new ValidationException($"Validation failed: {String.Join(";", ModelState.SelectMany(x => x.Value?.Errors.Select(z => $"{x.Key} : {z.ErrorMessage}") ?? new List<string>()))}");
            }

            var newEvent = new Event(ev.Title ?? "", ev.StartAt, ev.EndAt) { Description = ev.Description };
            eventsService.UpdateEvent(id, newEvent);
            return NoContent();
        }

        /// <summary>
        /// Deletes an event by ID from collection
        /// </summary>
        /// <param name="id">Id parameter of the event to delete</param>
        /// <response code="204">JSON-schema is returned of ApiResult with detailed responce
        /// and HTTP status-code 204 NoContent in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
        [HttpDelete("{id:Guid}")]
        public ActionResult DeleteEvent([FromRoute] Guid id)
        {
            eventsService.RemoveEvent(id);
            return NoContent();
        }
    }
}
