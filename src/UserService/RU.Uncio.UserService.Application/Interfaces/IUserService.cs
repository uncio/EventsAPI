using RU.Uncio.UserService.Domain.Models;

namespace RU.Uncio.UserService.Application.Interfaces
{
    /// <summary>
    /// User service interface
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Saves a user to repository
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> SaveUserAsync(User user, CancellationToken token);
        /// <summary>
        /// Returns a user from repository by ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> GetUserByIdAsync(Guid userId, CancellationToken token);
        /// <summary>
        /// Returns all users from repository
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<List<User>> GetAllUsersAsync(CancellationToken token);
        /// <summary>
        /// Verifies a user against DB
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<string> VerifyUserAsync(string login, string password, CancellationToken token);
    }
}
