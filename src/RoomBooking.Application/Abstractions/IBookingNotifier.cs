using RoomBooking.Application.DTOs;

namespace RoomBooking.Application.Abstractions;

public interface IBookingNotifier
{
    Task BookingChangedAsync(BookingDto booking, CancellationToken ct = default);
}