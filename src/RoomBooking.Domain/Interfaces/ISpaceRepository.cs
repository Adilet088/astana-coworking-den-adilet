using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Domain.Interfaces;

public interface ISpaceRepository
{
    Task<Space?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Space>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Space>> GetByCategoryAsync(SpaceCategory category, CancellationToken ct = default);
    Task<IReadOnlyList<Space>> GetAccessibleForRoleAsync(UserRole role, CancellationToken ct = default);
    Task<IReadOnlyList<Equipment>> GetAllEquipmentAsync(CancellationToken ct = default);
    Task AddAsync(Space space, CancellationToken ct = default);
    Task AddWithEquipmentAsync(Space space, IReadOnlyCollection<string> equipmentNames, CancellationToken ct = default);
    Task UpdateAsync(Space space, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}