using RoomBooking.Application.Abstractions;
using RoomBooking.Application.Common;
using RoomBooking.Application.DTOs;
using RoomBooking.Application.Mappings;
using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;
using RoomBooking.Domain.Interfaces;

namespace RoomBooking.Application.Services;

public sealed class SpaceService : ISpaceService
{
    private readonly ISpaceRepository _spaces;
    private readonly IUserRepository _users;

    public SpaceService(ISpaceRepository spaces, IUserRepository users)
    {
        _spaces = spaces;
        _users = users;
    }

    public async Task<Result<IReadOnlyList<SpaceDto>>> GetAccessibleAsync(
        Guid userId, SpaceCategory? category = null, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Errors.UserNotFound;

        var spaces = await _spaces.GetAccessibleForRoleAsync(user.Role, ct);

        var dtos = spaces
            .Where(s => category is null || s.Category == category)
            .Select(s => s.ToDto())
            .ToList();

        return Result.Success<IReadOnlyList<SpaceDto>>(dtos);
    }

    public async Task<Result<IReadOnlyList<SpaceDto>>> GetAllForAdminAsync(Guid adminId, CancellationToken ct = default)
    {
        var admin = await _users.GetByIdAsync(adminId, ct);
        if (admin is null)
            return Errors.UserNotFound;
        if (admin.Role != UserRole.Admin)
            return Errors.AdminOnly;

        var spaces = await _spaces.GetAllAsync(ct);
        var dtos = spaces.Select(s => s.ToDto()).ToList();
        return Result.Success<IReadOnlyList<SpaceDto>>(dtos);
    }

    public async Task<Result<IReadOnlyList<string>>> GetEquipmentCatalogAsync(CancellationToken ct = default)
    {
        var equipment = await _spaces.GetAllEquipmentAsync(ct);
        var names = equipment.Select(e => e.Name).ToList();
        return Result.Success<IReadOnlyList<string>>(names);
    }

    public async Task<Result<SpaceDto>> GetByIdAsync(Guid userId, Guid spaceId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Errors.UserNotFound;

        var space = await _spaces.GetByIdAsync(spaceId, ct);
        if (space is null)
            return Errors.SpaceNotFound;

        if (!AccessPolicy.CanView(user, space))
            return Errors.SpaceAccessDenied;

        return Result.Success(space.ToDto());
    }

    public async Task<Result<SpaceDto>> CreateAsync(Guid adminId, CreateSpaceRequest request, CancellationToken ct = default)
    {
        var admin = await _users.GetByIdAsync(adminId, ct);
        if (admin is null)
            return Errors.UserNotFound;
        if (admin.Role != UserRole.Admin)
            return Errors.AdminOnly;

        var name = request.Name?.Trim() ?? string.Empty;
        var location = request.Location?.Trim() ?? string.Empty;

        if (name.Length == 0)
            return new Error("Space.NameRequired", "Space name is required.");
        if (location.Length == 0)
            return new Error("Space.LocationRequired", "Location address is required.");
        if (request.Capacity <= 0 || request.Capacity > 1000)
            return new Error("Space.InvalidCapacity", "Capacity must be between 1 and 1000.");

        var roles = request.AllowedRoles.Count > 0
            ? request.AllowedRoles.Distinct().ToList()
            : new List<UserRole> { UserRole.Admin };

        if (!roles.Contains(UserRole.Admin))
            roles.Add(UserRole.Admin);

        var space = new Space
        {
            Name = name,
            Location = location,
            Category = request.Category,
            Capacity = request.Capacity,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            AllowedRoles = roles,
            IsActive = true
        };

        await _spaces.AddWithEquipmentAsync(space, request.Equipment, ct);
        return Result.Success(space.ToDto());
    }

    public async Task<Result> SetActiveAsync(Guid adminId, Guid spaceId, bool isActive, CancellationToken ct = default)
    {
        var admin = await _users.GetByIdAsync(adminId, ct);
        if (admin is null)
            return Result.Failure(Errors.UserNotFound);

        if (admin.Role != UserRole.Admin)
            return Result.Failure(Errors.AdminOnly);

        var space = await _spaces.GetByIdAsync(spaceId, ct);
        if (space is null)
            return Result.Failure(Errors.SpaceNotFound);

        space.IsActive = isActive;
        await _spaces.UpdateAsync(space, ct);

        return Result.Success();
    }
}