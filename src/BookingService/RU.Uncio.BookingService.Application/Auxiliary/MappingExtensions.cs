using RU.Uncio.BookingService.Application.DTO;
using RU.Uncio.BookingService.Domain.Models;

namespace RU.Uncio.BookingService.Application.Auxiliary
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
    }
}
