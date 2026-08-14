using Microsoft.EntityFrameworkCore;
using RU.Uncio.BookingService.Domain.Models;

namespace RU.Uncio.BookingService.Infrastructure.DataAccess
{
    /// <summary>
    /// DataBase context
    /// </summary>
    public class AppDbContext: DbContext
    {
        /// <summary>
        /// Bookings table
        /// </summary>
        public DbSet<Booking> Bookings => Set<Booking>();

        /// <summary>
        /// Users table
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
