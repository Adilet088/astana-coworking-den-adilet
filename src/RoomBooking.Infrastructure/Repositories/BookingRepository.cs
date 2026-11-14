using Microsoft.EntityFrameworkCore;
using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;
using RoomBooking.Domain.Interfaces;
using RoomBooking.Infrastructure.Persistence;

namespace RoomBooking.Infrastructure.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;

    public BookingRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Bookings
            .Include(b => b.Space)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<Booking>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await _db.Bookings.AsNoTracking()
            .Include(b => b.Space)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.StartTimeUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetBySpaceAsync(
        Guid spaceId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default) =>
        await _db.Bookings.AsNoTracking()
            .Include(b => b.Space)
            .Include(b => b.User)
            .Where(b => b.SpaceId == spaceId && b.StartTimeUtc < toUtc && fromUtc < b.EndTimeUtc)
            .OrderBy(b => b.StartTimeUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetAllInRangeAsync(
        DateTime fromUtc, DateTime toUtc, CancellationToken ct = default) =>
        await _db.Bookings.AsNoTracking()
            .Include(b => b.Space)
            .Include(b => b.User)
            .Where(b => b.StartTimeUtc < toUtc && fromUtc < b.EndTimeUtc)
            .OrderByDescending(b => b.StartTimeUtc)
            .ToListAsync(ct);

    public Task<bool> HasOverlapAsync(
        Guid spaceId, DateTime startUtc, DateTime endUtc, Guid? excludeBookingId = null, CancellationToken ct = default) =>
        _db.Bookings.AnyAsync(b =>
            b.SpaceId == spaceId &&
            b.Status == BookingStatus.Confirmed &&
            b.StartTimeUtc < endUtc &&
            startUtc < b.EndTimeUtc &&
            (excludeBookingId == null || b.Id != excludeBookingId), ct);

    public async Task AddAsync(Booking booking, CancellationToken ct = default)
    {
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        if (_db.Entry(booking).State == EntityState.Detached)
            _db.Bookings.Update(booking);
        await _db.SaveChangesAsync(ct);
    }
}