using RoomBooking.Application.Common;
using RoomBooking.Application.DTOs;

namespace RoomBooking.Application.Abstractions;

public interface IBookingService
{
    Task<Result<BookingDto>> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct = default);
    Task<Result> CancelAsync(Guid userId, Guid bookingId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<BookingDto>>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<BookingDto>>> GetScheduleAsync(Guid userId, Guid spaceId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<Result<IReadOnlyList<BookingDto>>> GetAllBookingsForAdminAsync(Guid adminId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
}