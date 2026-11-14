using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using RoomBooking.Application.DTOs;
using RoomBooking.Web.Hubs;
using RoomBooking.Web.State;

namespace RoomBooking.Web.Notifications;

/// <summary>Per-circuit SignalR client that feeds incoming changes into the state container.</summary>
public sealed class BookingHubClient(NavigationManager navigation, BookingStateContainer state) : IAsyncDisposable
{
    private HubConnection? _connection;

    public async Task StartAsync()
    {
        if (_connection is not null) return;

        _connection = new HubConnectionBuilder()
            .WithUrl(navigation.ToAbsoluteUri(BookingHub.Path))
            .WithAutomaticReconnect()
            .Build();

        _connection.On<BookingDto>(BookingHub.BookingChangedMethod, state.ApplyBookingChange);
        await _connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}