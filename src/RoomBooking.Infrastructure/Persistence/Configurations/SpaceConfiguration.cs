using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Infrastructure.Persistence.Configurations;

public sealed class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Location).IsRequired().HasMaxLength(300);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.Category).HasConversion<string>().HasMaxLength(30);

        builder.Property(s => s.AllowedRoles)
            .HasConversion(
                v => string.Join(",", v.Select(r => ((int)r).ToString())),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => (UserRole)int.Parse(x))
                      .ToList())
            .HasMaxLength(50);

        builder.Property(s => s.AllowedRoles).Metadata.SetValueComparer(
            new ValueComparer<List<UserRole>>(
                (a, b) => a!.SequenceEqual(b!),
                l => l.Aggregate(0, (h, r) => HashCode.Combine(h, (int)r)),
                l => l.ToList()));

        builder.HasMany(s => s.Equipment)
            .WithMany(e => e.Spaces)
            .UsingEntity<Dictionary<string, object>>(
                "SpaceEquipment",
                r => r.HasOne<Equipment>().WithMany().HasForeignKey("EquipmentId"),
                l => l.HasOne<Space>().WithMany().HasForeignKey("SpaceId"),
                j =>
                {
                    j.HasKey("SpaceId", "EquipmentId");
                    j.ToTable("SpaceEquipment");
                });
    }
}