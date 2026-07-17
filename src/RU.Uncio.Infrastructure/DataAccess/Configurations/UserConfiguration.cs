using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RU.Uncio.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RU.Uncio.Infrastructure.DataAccess.Configurations
{
    /// <summary>
    /// DB on create configuration
    /// </summary>
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.Login).IsRequired();
            builder.HasIndex(p => p.Login).IsUnique();
            builder.Property(p => p.HashedPassword).IsRequired();
            builder.Property(p => p.Name).HasMaxLength(200);
            builder.Property(p => p.Name).HasDefaultValue("Not set");

            builder.Property(p => p.Role).HasConversion<string>();

            builder.HasMany(o => o.Bookings)
                   .WithOne(u => u.User)
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
