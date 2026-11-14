using RoomBooking.Domain.Enums;

namespace RoomBooking.Application.DTOs;

public sealed record SpaceDto(
    Guid Id,
    string Name,
    string Location,
    SpaceCategory Category,
    int Capacity,
    string? Description,
    bool IsActive,
    IReadOnlyList<string> Equipment,
    IReadOnlyList<UserRole> AllowedRoles);