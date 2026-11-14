using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Web.Auth;

public sealed record DemoUser(Guid Id, string Email, string FullName, UserRole Role);

public sealed class DemoAuthService(ProtectedSessionStorage sessionStorage)
{
    private const string SessionKey = "rb_demo_user_id";

    public static readonly IReadOnlyList<DemoUser> Users =
    [
        new(Guid.Parse("10000000-0000-0000-0000-000000000001"), "student@roombooking.kz", "Deniz Akhmadjanov", UserRole.Student),
        new(Guid.Parse("10000000-0000-0000-0000-000000000002"), "teacher@roombooking.kz", "Tr. Adilet", UserRole.Teacher),
        new(Guid.Parse("10000000-0000-0000-0000-000000000003"), "admin@roombooking.kz", "System Administrator", UserRole.Admin)
    ];

    private readonly Dictionary<string, string> _passwords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["student@roombooking.kz"] = "Student123!",
        ["teacher@roombooking.kz"] = "Teacher123!",
        ["admin@roombooking.kz"] = "Admin123!"
    };

    private bool _initialized;

    public event Action? OnUserChanged;

    public DemoUser? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;
    public Guid CurrentUserId => CurrentUser?.Id ?? Guid.Empty;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public string GetDemoPassword(string email) =>
        _passwords.TryGetValue(email, out var pwd) ? pwd : string.Empty;

    public async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var stored = await sessionStorage.GetAsync<Guid>(SessionKey);
            if (stored.Success)
            {
                var user = Users.FirstOrDefault(u => u.Id == stored.Value);
                if (user is not null)
                {
                    CurrentUser = user;
                    OnUserChanged?.Invoke();
                }
            }
        }
        catch
        {
            // Ignore prerender JS interop unavailability
        }
    }

    public async Task<(bool Success, string? Error)> SignInAsync(string email, string password)
    {
        var cleanEmail = email.Trim();
        var user = Users.FirstOrDefault(u => u.Email.Equals(cleanEmail, StringComparison.OrdinalIgnoreCase));

        if (user is null || !_passwords.TryGetValue(user.Email, out var expected) || expected != password)
            return (false, "Invalid email or password.");

        CurrentUser = user;
        _initialized = true;
        await sessionStorage.SetAsync(SessionKey, user.Id);
        OnUserChanged?.Invoke();
        return (true, null);
    }

    public (bool Success, string? Error) ResetPassword(string email, string newPassword)
    {
        var cleanEmail = email.Trim();
        var user = Users.FirstOrDefault(u => u.Email.Equals(cleanEmail, StringComparison.OrdinalIgnoreCase));
        if (user is null)
            return (false, "No account found with this email address.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "New password must be at least 6 characters.");

        _passwords[user.Email] = newPassword;
        return (true, null);
    }

    public async Task SignOutAsync()
    {
        CurrentUser = null;
        await sessionStorage.DeleteAsync(SessionKey);
        OnUserChanged?.Invoke();
    }
}