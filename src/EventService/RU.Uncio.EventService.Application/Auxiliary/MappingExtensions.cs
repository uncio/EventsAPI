using RU.Uncio.EventService.Application.DTO;
using RU.Uncio.EventService.Domain.Models;

namespace RU.Uncio.EventService.Application.Auxiliary
{
    /// <summary>
    /// Extensions to map models to dto
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Mapper for event
        /// </summary>
        /// <param name="mappingObject"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static EventDTO MapToDto(this Event? mappingObject)
        {
            if (mappingObject == null)
                throw new ArgumentNullException(nameof(mappingObject));
            EventDTO dest = new()
            {
                Id = mappingObject.Id,
                Title = mappingObject.Title,
                Description = mappingObject.Description,
                StartAt = mappingObject.StartAt,
                EndAt = mappingObject.EndAt,
                TotalSeats = mappingObject.TotalSeats,
                AvailableSeats = mappingObject.AvailableSeats,
            };

            return dest;
        }
    }
}
