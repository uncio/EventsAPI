using Microsoft.EntityFrameworkCore;
using RU.Uncio.UserService.Domain.Models;

namespace RU.Uncio.UserService.Infrastructure.DataAccess
{
    /// <summary>
    /// DataBase context
    /// </summary>
    public class AppDbContext: DbContext
    {
        /// <summary>
        /// Users table
        /// </summary>
        public DbSet<User> Users => Set<User>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opts"></param>
        public AppDbContext(DbContextOptions<AppDbContext> opts)
            :base(opts)
        {
        }

        /// <summary>
        /// On model creating setup
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
