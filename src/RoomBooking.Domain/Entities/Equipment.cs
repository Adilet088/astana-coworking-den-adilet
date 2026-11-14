using RoomBooking.Domain.Common;

namespace RoomBooking.Domain.Entities;

public class Equipment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Space> Spaces { get; set; } = new List<Space>();
}