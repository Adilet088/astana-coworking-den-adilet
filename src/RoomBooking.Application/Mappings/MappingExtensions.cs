using RoomBooking.Application.DTOs;
using RoomBooking.Domain.Entities;

namespace RoomBooking.Application.Mappings;

public static class MappingExtensions
{
    public static SpaceDto ToDto(this Space space) => new(
        space.Id,
        space.Name,
        space.Location,
        space.Category,
        space.Capacity,
        space.Description,
        space.IsActive,
        space.Equipment.Select(e => e.Name).OrderBy(n => n).ToList(),
        space.AllowedRoles.ToList());

    public static BookingDto ToDto(this Booking booking) => new(
        booking.Id,
        booking.SpaceId,
        booking.Space.Name,
        booking.UserId,
        booking.User.FullName,
        booking.StartTimeUtc,
        booking.EndTimeUtc,
        booking.Purpose,
        booking.Status);
}