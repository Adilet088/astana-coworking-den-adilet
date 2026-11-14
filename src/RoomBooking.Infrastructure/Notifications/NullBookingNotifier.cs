using RoomBooking.Application.Abstractions;
using RoomBooking.Application.DTOs;

namespace RoomBooking.Infrastructure.Notifications;

public sealed class NullBookingNotifier : IBookingNotifier
{
    public Task BookingChangedAsync(BookingDto booking, CancellationToken ct = default) => Task.CompletedTask;
}