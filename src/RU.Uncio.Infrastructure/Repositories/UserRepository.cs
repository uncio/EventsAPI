using Microsoft.EntityFrameworkCore;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.Domain.Models;
using RU.Uncio.Infrastructure.DataAccess;

namespace RU.Uncio.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete users repository
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext db;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dB"></param>
        public UserRepository(AppDbContext dB) { db = dB; }
        /// <summary>
        /// Adds a user to DB
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<User> AddUserAsync(User user, CancellationToken token)
        {
            var result = await db.Users.AddAsync(user, token);
            await db.SaveChangesAsync(token);

            return result.Entity;
        }
        /// <summary>
        /// Returns all users from DB
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<User>> GetAllUsersAsync(CancellationToken token)
        {
            return await db.Users.Include(u => u.Bookings).ToListAsync();
        }
        /// <summary>
        /// Gets a user from DB by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<User> GetUserAsync(Guid id, CancellationToken token)
        {
            return await db.Users.Include(u => u.Bookings).FirstOrDefaultAsync(u => u.Id.Equals(id), token);
        }
        /// <summary>
        /// Gets a user from DB by Login
        /// </summary>
        /// <param name="login"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<User> GetUserByLoginAsync(string login, CancellationToken token)
        {
            return await db.Users.Include(u => u.Bookings).FirstOrDefaultAsync(u => u.Login.Equals(login), token);
        }
    }
}
