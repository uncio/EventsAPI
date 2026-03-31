using Microsoft.AspNetCore.Mvc;
using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using System.Net;

namespace RU.Uncio.EventsAPI.Controllers
{
    /// <summary>
    /// Events controller
    /// </summary>
    /// <param name="eventsService">Constructor with service</param>
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController(IEventsService eventsService) : ControllerBase
    {
        /// <summary>
        /// Returns all events from collection
        /// </summary>
        /// <response code="200">JSON-schema of ApiResult is returned with events and detailed responce
        /// and HTTP status-code 200 Ok in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        public ApiResult<List<Event>> GetAllEvents()
        {
            return new ApiResult<List<Event>>
            {
                Data = eventsService.GetEvents(),
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Gettin all events from collection"
            };
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
        public ApiBaseResult GetBuildingByIndex(Guid id)
        {
            var result = eventsService.GetEvent(id);

            if(result != null)
            {
                return new ApiResult<Event>
                {
                    Data = eventsService.GetEvent(id),
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = $"Getting event with ID {id} from collection"
                };
            }
            else
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Event with ID {id} is not found in the collection"
                };
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
        public ApiResult CreateEvent([FromBody] EventDTO ev)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Model validation error"
                    //how to send ModelState here?
                };
            }

            try
            {
                var newEvent = new Event(ev.Id, ev.Title, ev.StartAt, ev.EndAt) { Description = ev.Description };
                eventsService.AddEvent(newEvent);

                return new ApiResult
                {
                    Success = true,
                    StatusCode = HttpStatusCode.Created,
                    Message = "Adding the event to the collection"
                };
            }
            catch(ArgumentException ex)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = ex.Message
                };
            }            
        }

        /// <summary>
        /// Replaces an event from request body by ID in collection
        /// </summary>
        /// <param name="id">Id parameter to replace an event</param>
        /// <param name="ev">Event from request body to replace</param>
        /// <response code="204">JSON-schema is returned of ApiResult with detailed responce
        /// and HTTP status-code 204 NoContent in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
        [Consumes("application/json")]
        [HttpPut("{id:Guid}")]
        public ApiResult ReplaceEvent(Guid id, [FromBody] EventDTO ev)
        {
            if (!ModelState.IsValid)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Model validation error"
                    //how to send ModelState here?
                };
            }

            try
            {
                var newEvent = new Event(ev.Id, ev.Title, ev.StartAt, ev.EndAt) { Description = ev.Description };
                eventsService.ReplaceEvent(id, newEvent);
                return new ApiResult
                {
                    Success = true,
                    StatusCode = HttpStatusCode.NoContent,
                    Message = $"Replacing the event by ID {ev.Id}"
                };
            }            
            catch (ArgumentException ex)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = ex.Message
                };
            }
            catch (IndexOutOfRangeException)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Event with ID {id} is not found in the collection"
                };
            }
        }

        /// <summary>
        /// Deletes an event by ID from collection
        /// </summary>
        /// <param name="id">Id parameter of the event to delete</param>
        /// <response code="204">JSON-schema is returned of ApiResult with detailed responce
        /// and HTTP status-code 204 NoContent in case of success</response>
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
        [HttpDelete("{id:Guid}")]
        public ApiResult DeleteEvent(Guid id)
        {
            try
            {
                eventsService.RemoveEvent(id);
                return new ApiResult
                {
                    Success = true,
                    StatusCode = HttpStatusCode.NoContent,
                    Message = $"Deleting event with ID {id}"
                };
            }
            catch (IndexOutOfRangeException)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Event with ID {id} is not found in the collection"
                };
            }
            
        }
    }
}
