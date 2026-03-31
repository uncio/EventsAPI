using Microsoft.AspNetCore.Mvc;
using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Helpers;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;
using System.Net;

namespace RU.Uncio.EventsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController(IEventsService eventsService) : ControllerBase
    {
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

        [HttpPost]
        public ApiResult CreateEvent([FromBody] EventDTO ev)
        {
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

        [HttpPut("{id:Guid}")]
        public ApiResult ReplaceEvent(Guid id, [FromBody] EventDTO ev)
        {
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
