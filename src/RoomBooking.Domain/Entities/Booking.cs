using RoomBooking.Domain.Common;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid SpaceId { get; set; }
    public Space Space { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public bool OverlapsWith(DateTime startUtc, DateTime endUtc) =>
        StartTimeUtc < endUtc && startUtc < EndTimeUtc;

    public void Cancel()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be cancelled.");
        Status = BookingStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be completed.");
        Status = BookingStatus.Completed;
    }
}