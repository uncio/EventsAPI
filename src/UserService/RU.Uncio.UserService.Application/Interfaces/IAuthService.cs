using RU.Uncio.UserService.Domain.Models;

namespace RU.Uncio.UserService.Application.Interfaces
{
    /// <summary>
    /// Token service interface
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Create token for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string CreateToken(User user);
    }
}
