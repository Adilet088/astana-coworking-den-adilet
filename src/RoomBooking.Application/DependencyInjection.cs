using Microsoft.Extensions.DependencyInjection;
using RoomBooking.Application.Abstractions;
using RoomBooking.Application.Services;

namespace RoomBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ISpaceService, SpaceService>();
        services.AddScoped<IBookingService, BookingService>();
        return services;
    }
}