using Microsoft.EntityFrameworkCore;
using RoomBooking.Domain.Entities;

namespace RoomBooking.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData.Apply(modelBuilder);
    }
}