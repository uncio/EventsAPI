using Microsoft.AspNetCore.Mvc;
using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController(IEventsService eventsService) : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<Event>> GetAllEvents()
        {
            return eventsService.GetEvents();
        }

        [HttpGet("{id:Guid}")]
        public ActionResult<Event> GetBuildingByIndex(Guid id)
        {
            return eventsService.GetEvent(id);
        }

        [HttpPost]
        public IActionResult CreateEvent([FromBody] EventDTO ev)
        {
            try
            {
                var newEvent = new Event(ev.Id, ev.Title, ev.StartAt, ev.EndAt) { Description = ev.Description };
                eventsService.AddEvent(newEvent);
                return CreatedAtAction(nameof(CreateEvent), newEvent);
            }
            catch(ArgumentException)
            {
                return BadRequest();
            }
            
        }

        [HttpPut("{id:Guid}")]
        public IActionResult ReplaceEvent(Guid id, [FromBody] EventDTO ev)
        {
            try
            {
                var newEvent = new Event(ev.Id, ev.Title, ev.StartAt, ev.EndAt) { Description = ev.Description };
                eventsService.ReplaceEvent(id, newEvent);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }           
        }

        [HttpDelete("{id:Guid}")]
        public IActionResult DeleteEvent(Guid id)
        {
            eventsService.RemoveEvent(id);
            return Ok();
        }
    }
}
