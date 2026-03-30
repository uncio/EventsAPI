using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Post([FromBody] Event ev)
        {
            eventsService.AddEvent(ev);
            return new CreatedResult();
        }

        [HttpPut("{id:Guid}")]
        public IActionResult Put(Guid id, [FromBody] Event ev)
        {
            eventsService.ReplaceEvent(id, ev);
            return new NoContentResult();
        }

        [HttpDelete("{id:Guid}")]
        public IActionResult Delete(Guid id)
        {
            eventsService.RemoveEvent(id);
            return new OkResult();
        }
    }
}
