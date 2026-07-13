using Microsoft.EntityFrameworkCore;
using RU.Uncio.Domain.Models;

namespace RU.Uncio.Infrastructure.DataAccess
{
    /// <summary>
    /// DataBase context
    /// </summary>
    public class AppDbContext: DbContext
    {
        /// <summary>
        /// Events table
        /// </summary>
        public DbSet<Event> Events => Set<Event>();
        /// <summary>
        /// Bookings table
        /// </summary>
        public DbSet<Booking> Bookings => Set<Booking>();

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
