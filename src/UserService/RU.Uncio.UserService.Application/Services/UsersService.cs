using Microsoft.Extensions.Logging;
using RU.Uncio.UserService.Application.Auxiliary;
using RU.Uncio.UserService.Application.Interfaces;
using RU.Uncio.UserService.Domain.Models;

namespace RU.Uncio.UserService.Application.Services
{
    /// <summary>
    /// User service
    /// </summary>
    public class UsersService : IUserService
    {
        private readonly ILogger<UsersService> logger;
        private readonly IUserRepository repository;
        private readonly IAuthService authService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="repo"></param>
        /// <param name="auth"></param>
        public UsersService(ILogger<UsersService> log, IUserRepository repo, IAuthService auth)
        {
            logger = log;
            repository = repo;
            authService = auth;
        }

        /// <summary>
        /// Returns all users from repository
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<User>> GetAllUsersAsync(CancellationToken token)
        {
            return await repository.GetAllUsersAsync(token);
        }

        /// <summary>
        /// Returns a user from repository by ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<User> GetUserByIdAsync(Guid userId, CancellationToken token)
        {
            var user = await repository.GetUserAsync(userId, token);
            return user;
        }

        /// <summary>
        /// Saves a user to repository
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<User> SaveUserAsync(User user, CancellationToken token)
        {
            var addedUser = await repository.AddUserAsync(user, token);
            return addedUser;
        }

        /// <summary>
        /// Verifies a user against DB
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> VerifyUserAsync(string login, string password, CancellationToken token)
        {
            var user = await repository.GetUserByLoginAsync(login, token);
            if (PasswordHasher.VerifyPassword(password, user.HashedPassword))
            {
                return authService.CreateToken(user);
            }

            return null;
        }
    }
}
