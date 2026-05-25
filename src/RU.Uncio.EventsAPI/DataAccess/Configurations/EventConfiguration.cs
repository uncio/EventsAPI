using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RU.Uncio.EventsAPI.Models;

namespace RU.Uncio.EventsAPI.DataAccess.Configurations
{
    public class EventConfiguration: IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("events");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.Title).IsRequired();
            builder.Property(p => p.Title).HasMaxLength(200);
            builder.Property(p => p.StartAt).IsRequired();
            builder.Property(p => p.EndAt).IsRequired();
            builder.Property(p => p.TotalSeats).IsRequired();

            builder.HasMany(o => o.Bookings)
                   .WithOne(u => u.Event)
                   .HasForeignKey(o => o.EventId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
