using Microsoft.EntityFrameworkCore;
using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Interfaces;
using RoomBooking.Infrastructure.Persistence;

namespace RoomBooking.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        user.Email = user.Email.Trim().ToLowerInvariant();
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        user.Email = user.Email.Trim().ToLowerInvariant();
        if (_db.Entry(user).State == EntityState.Detached)
            _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
    }
}