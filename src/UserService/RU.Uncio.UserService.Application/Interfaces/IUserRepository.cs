using RU.Uncio.UserService.Domain.Models;

namespace RU.Uncio.UserService.Application.Interfaces
{
    /// <summary>
    /// User repository interface
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user from DB by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> GetUserAsync(Guid id, CancellationToken token);
        /// <summary>
        /// Gets a user from DB by Login
        /// </summary>
        /// <param name="login"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> GetUserByLoginAsync(string login, CancellationToken token);
        /// <summary>
        /// Adds a user to DB
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<User> AddUserAsync(User user, CancellationToken token);
        /// <summary>
        /// Returns all users from DB
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<List<User>> GetAllUsersAsync(CancellationToken token);
    }
}
