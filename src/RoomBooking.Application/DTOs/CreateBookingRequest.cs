namespace RoomBooking.Application.DTOs;

public sealed class CreateBookingRequest
{
    public Guid SpaceId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string Purpose { get; set; } = string.Empty;
}