using RoomBooking.Domain.Common;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Domain.Entities;

public class Space : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public SpaceCategory Category { get; set; }
    public int Capacity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    public List<UserRole> AllowedRoles { get; set; } = new();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public bool IsAccessibleTo(UserRole role) => IsActive && AllowedRoles.Contains(role);
}