using RoomBooking.Domain.Enums;

namespace RoomBooking.Application.Services;

internal static class BookingPolicy
{
    public const int MaxPurposeLength = 200;
    public static readonly TimeSpan MinDuration = TimeSpan.FromMinutes(15);

    public static TimeSpan MaxDuration(UserRole role) => role switch
    {
        UserRole.Student => TimeSpan.FromHours(3),
        UserRole.Teacher => TimeSpan.FromHours(8),
        _ => TimeSpan.FromHours(12)
    };
}