using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.Auxiliary
{
    /// <summary>
    /// Extensions to map models to dto
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Mapper for Booking
        /// </summary>
        /// <param name="mappingObject"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static BookingDTO MapToDto(this Booking? mappingObject)
        {
            if (mappingObject == null)
                throw new ArgumentNullException(nameof(mappingObject));
            BookingDTO dest = new()
            {
                Id = mappingObject.Id,
                EventId = mappingObject.EventId,
                Status = mappingObject.Status.ToString(),
                CreatedAt = mappingObject.CreatedAt,
                ProcessedAt = mappingObject.ProcessedAt,
            };

            return dest;
        }
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
                EndAt = mappingObject.EndAt
            };

            return dest;
        }
    }
}
