using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoomBooking.Application.Abstractions;
using RoomBooking.Domain.Interfaces;
using RoomBooking.Infrastructure.Notifications;
using RoomBooking.Infrastructure.Persistence;
using RoomBooking.Infrastructure.Repositories;

namespace RoomBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISpaceRepository, SpaceRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBookingNotifier, NullBookingNotifier>();

        return services;
    }
}