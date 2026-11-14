using RoomBooking.Application.Common;
using RoomBooking.Application.DTOs;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Application.Abstractions;

public interface ISpaceService
{
    Task<Result<IReadOnlyList<SpaceDto>>> GetAccessibleAsync(Guid userId, SpaceCategory? category = null, CancellationToken ct = default);
    Task<Result<IReadOnlyList<SpaceDto>>> GetAllForAdminAsync(Guid adminId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<string>>> GetEquipmentCatalogAsync(CancellationToken ct = default);
    Task<Result<SpaceDto>> GetByIdAsync(Guid userId, Guid spaceId, CancellationToken ct = default);
    Task<Result<SpaceDto>> CreateAsync(Guid adminId, CreateSpaceRequest request, CancellationToken ct = default);
    Task<Result> SetActiveAsync(Guid adminId, Guid spaceId, bool isActive, CancellationToken ct = default);
}