using Microsoft.AspNetCore.SignalR;

namespace RoomBooking.Web.Hubs;

public sealed class BookingHub : Hub
{
    public const string Path = "/hubs/bookings";
    public const string BookingChangedMethod = "BookingChanged";
}