using Microsoft.AspNetCore.SignalR;
using RoomBooking.Application.Abstractions;
using RoomBooking.Application.DTOs;
using RoomBooking.Web.Hubs;

namespace RoomBooking.Web.Notifications;

public sealed class SignalRBookingNotifier(IHubContext<BookingHub> hub) : IBookingNotifier
{
    public Task BookingChangedAsync(BookingDto booking, CancellationToken ct = default)
        => hub.Clients.All.SendAsync(BookingHub.BookingChangedMethod, booking, ct);
}