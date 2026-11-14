using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace RoomBooking.Web.Auth;

public sealed class DemoAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly DemoAuthService _auth;

    public DemoAuthenticationStateProvider(DemoAuthService auth)
    {
        _auth = auth;
        _auth.OnUserChanged += HandleUserChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _auth.CurrentUser;
        if (user is null)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, "DemoAuth");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private void HandleUserChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void Dispose() => _auth.OnUserChanged -= HandleUserChanged;
}