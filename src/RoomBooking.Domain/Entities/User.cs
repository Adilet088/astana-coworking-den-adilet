using RoomBooking.Domain.Common;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}