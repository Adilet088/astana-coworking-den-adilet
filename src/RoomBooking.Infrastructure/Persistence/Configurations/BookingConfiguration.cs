using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.Domain.Entities;

namespace RoomBooking.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Purpose).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(b => b.Space)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.SpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.SpaceId, b.StartTimeUtc, b.EndTimeUtc });
    }
}