using RoomBooking.Domain.Enums;

namespace RoomBooking.Application.DTOs;

public sealed class CreateSpaceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public SpaceCategory Category { get; set; } = SpaceCategory.CoworkingOpenSpace;
    public int Capacity { get; set; } = 20;
    public string? Description { get; set; }
    public List<string> Equipment { get; set; } = new();
    public List<UserRole> AllowedRoles { get; set; } = new() { UserRole.Student, UserRole.Teacher, UserRole.Admin };
}