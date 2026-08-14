using RU.Uncio.UserService.Application.DTO;
using RU.Uncio.UserService.Domain.Models;

namespace RU.Uncio.UserService.Application.Auxiliary
{
    /// <summary>
    /// Extensions to map models to dto
    /// </summary>
    public static class MappingExtensions
    {        
        /// <summary>
        /// Mapper for user
        /// </summary>
        /// <param name="mappingObject"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static UserDTO MapToDto(this User? mappingObject)
        {
            if (mappingObject == null)
                throw new ArgumentNullException(nameof(mappingObject));
            UserDTO dest = new()
            {
                Id = mappingObject.Id,
                Name = mappingObject.Name,
                Login = mappingObject.Login,
                Role = mappingObject.Role,
                Password = ""
            };

            return dest;
        }
    }
}
