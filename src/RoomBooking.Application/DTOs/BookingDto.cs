using RoomBooking.Domain.Enums;

namespace RoomBooking.Application.DTOs;

public sealed record BookingDto(
    Guid Id,
    Guid SpaceId,
    string SpaceName,
    Guid UserId,
    string UserName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string Purpose,
    BookingStatus Status);