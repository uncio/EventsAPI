using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RU.Uncio.BookingService.Domain.Models;

namespace RU.Uncio.BookingService.Infrastructure.DataAccess.Configurations
{
    /// <summary>
    /// DB on create configuration
    /// </summary>
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.EventId).IsRequired();
            builder.Property(p => p.Status).HasConversion<string>();

            //builder.HasOne(o => o.Event)
            //       .WithMany(u => u.Bookings)
            //       .HasForeignKey(o => o.EventId);

            //builder.HasOne(o => o.User)
            //       .WithMany(u => u.Bookings)
            //       .HasForeignKey(o => o.UserId);
        }
    }
}
