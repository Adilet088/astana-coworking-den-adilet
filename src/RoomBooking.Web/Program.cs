using Microsoft.AspNetCore.Components.Authorization;
using RoomBooking.Application;
using RoomBooking.Application.Abstractions;
using RoomBooking.Infrastructure;
using RoomBooking.Infrastructure.Persistence;
using RoomBooking.Web.Auth;
using RoomBooking.Web.Components;
using RoomBooking.Web.Hubs;
using RoomBooking.Web.Notifications;
using RoomBooking.Web.State;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents(options =>
{
    options.DetailedErrors = true;
});
builder.Services.AddSignalR();

builder.Services.AddApplication();
builder.Services.AddInfrastructure("Data Source=roombooking.db");

// Override the null notifier with the SignalR implementation (last registration wins).
builder.Services.AddScoped<IBookingNotifier, SignalRBookingNotifier>();

builder.Services.AddScoped<BookingStateContainer>();
builder.Services.AddScoped<BookingHubClient>();

builder.Services.AddScoped<DemoAuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, DemoAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapHub<BookingHub>(BookingHub.Path);
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();