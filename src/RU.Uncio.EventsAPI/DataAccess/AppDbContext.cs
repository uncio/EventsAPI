using Microsoft.EntityFrameworkCore;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.DataAccess
{
    /// <summary>
    /// DataBase context
    /// </summary>
    public class AppDbContext: DbContext
    {
        private readonly ILogger<AppDbContext> logger;

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
        /// <param name="log"></param>
        public AppDbContext(DbContextOptions<AppDbContext> opts, ILogger<AppDbContext> log)
            :base(opts)
        {
            logger = log;
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
