using Microsoft.EntityFrameworkCore;
using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;
using RoomBooking.Domain.Interfaces;
using RoomBooking.Infrastructure.Persistence;

namespace RoomBooking.Infrastructure.Repositories;

public sealed class SpaceRepository : ISpaceRepository
{
    private readonly AppDbContext _db;

    public SpaceRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Space?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Spaces.Include(s => s.Equipment).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Space>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Spaces.AsNoTracking()
            .Include(s => s.Equipment)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Space>> GetByCategoryAsync(SpaceCategory category, CancellationToken ct = default) =>
        await _db.Spaces.AsNoTracking()
            .Include(s => s.Equipment)
            .Where(s => s.Category == category)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Space>> GetAccessibleForRoleAsync(UserRole role, CancellationToken ct = default)
    {
        var spaces = await _db.Spaces.AsNoTracking()
            .Include(s => s.Equipment)
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return spaces.Where(s => s.IsAccessibleTo(role)).ToList();
    }

    public async Task<IReadOnlyList<Equipment>> GetAllEquipmentAsync(CancellationToken ct = default) =>
        await _db.Equipment.AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Space space, CancellationToken ct = default)
    {
        _db.Spaces.Add(space);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddWithEquipmentAsync(Space space, IReadOnlyCollection<string> equipmentNames, CancellationToken ct = default)
    {
        if (equipmentNames.Count > 0)
        {
            var normalized = equipmentNames
                .Select(n => n.Trim())
                .Where(n => n.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existing = await _db.Equipment
                .Where(e => normalized.Contains(e.Name))
                .ToListAsync(ct);

            foreach (var name in normalized)
            {
                var eq = existing.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (eq is null)
                {
                    eq = new Equipment { Name = name };
                    _db.Equipment.Add(eq);
                }
                space.Equipment.Add(eq);
            }
        }

        _db.Spaces.Add(space);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Space space, CancellationToken ct = default)
    {
        if (_db.Entry(space).State == EntityState.Detached)
            _db.Spaces.Update(space);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var space = await _db.Spaces.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (space is null)
            return;

        _db.Spaces.Remove(space);
        await _db.SaveChangesAsync(ct);
    }
}