using RoomBooking.Domain.Entities;

namespace RoomBooking.Domain.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetBySpaceAsync(Guid spaceId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetAllInRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<bool> HasOverlapAsync(Guid spaceId, DateTime startUtc, DateTime endUtc, Guid? excludeBookingId = null, CancellationToken ct = default);
    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task UpdateAsync(Booking booking, CancellationToken ct = default);
}