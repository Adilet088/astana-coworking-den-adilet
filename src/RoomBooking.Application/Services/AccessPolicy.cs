using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Application.Services;

internal static class AccessPolicy
{
    public static bool CanView(User user, Space space) =>
        user.Role == UserRole.Admin || space.IsAccessibleTo(user.Role);
}