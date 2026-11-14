using RoomBooking.Application.Abstractions;
using RoomBooking.Application.Common;
using RoomBooking.Application.DTOs;
using RoomBooking.Application.Mappings;
using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;
using RoomBooking.Domain.Interfaces;

namespace RoomBooking.Application.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly ISpaceRepository _spaces;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookingNotifier _notifier;
    private readonly TimeProvider _time;

    public BookingService(
        IBookingRepository bookings,
        ISpaceRepository spaces,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IBookingNotifier notifier,
        TimeProvider time)
    {
        _bookings = bookings;
        _spaces = spaces;
        _users = users;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
        _time = time;
    }

    public async Task<Result<BookingDto>> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Errors.UserNotFound;

        var start = ToUtc(request.StartTimeUtc);
        var end = ToUtc(request.EndTimeUtc);
        var purpose = request.Purpose?.Trim() ?? string.Empty;
        var now = _time.GetUtcNow().UtcDateTime;

        if (purpose.Length == 0)
            return Errors.PurposeRequired;
        if (purpose.Length > BookingPolicy.MaxPurposeLength)
            return Errors.PurposeTooLong;
        if (end <= start)
            return Errors.InvalidTimeRange;
        if (start < now)
            return Errors.StartInPast;

        var duration = end - start;
        if (duration < BookingPolicy.MinDuration)
            return Errors.DurationTooShort(BookingPolicy.MinDuration);

        var maxDuration = BookingPolicy.MaxDuration(user.Role);
        if (duration > maxDuration)
            return Errors.DurationTooLong(maxDuration);

        var space = await _spaces.GetByIdAsync(request.SpaceId, ct);
        if (space is null)
            return Errors.SpaceNotFound;
        if (!space.IsActive)
            return Errors.SpaceInactive;
        if (!space.IsAccessibleTo(user.Role))
            return Errors.SpaceAccessDenied;

        var result = await _unitOfWork.ExecuteInTransactionAsync<Result<BookingDto>>(async () =>
        {
            if (await _bookings.HasOverlapAsync(space.Id, start, end, null, ct))
                return Errors.SlotTaken;

            var booking = new Booking
            {
                SpaceId = space.Id,
                UserId = user.Id,
                StartTimeUtc = start,
                EndTimeUtc = end,
                Purpose = purpose,
                Status = BookingStatus.Confirmed
            };

            await _bookings.AddAsync(booking, ct);

            booking.Space = space;
            booking.User = user;

            return Result.Success(booking.ToDto());
        }, ct);

        if (result.IsSuccess)
            await _notifier.BookingChangedAsync(result.Value, ct);

        return result;
    }

    public async Task<Result> CancelAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure(Errors.UserNotFound);

        var booking = await _bookings.GetByIdAsync(bookingId, ct);
        if (booking is null)
            return Result.Failure(Errors.BookingNotFound);

        if (booking.UserId != user.Id && user.Role != UserRole.Admin)
            return Result.Failure(Errors.BookingForbidden);

        var now = _time.GetUtcNow().UtcDateTime;
        if (booking.Status != BookingStatus.Confirmed || booking.EndTimeUtc <= now)
            return Result.Failure(Errors.BookingNotCancellable);

        booking.Cancel();
        await _bookings.UpdateAsync(booking, ct);
        await _notifier.BookingChangedAsync(booking.ToDto(), ct);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<BookingDto>>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Errors.UserNotFound;

        var bookings = await _bookings.GetByUserAsync(userId, ct);
        var dtos = bookings.Select(b => b.ToDto()).ToList();

        return Result.Success<IReadOnlyList<BookingDto>>(dtos);
    }

    public async Task<Result<IReadOnlyList<BookingDto>>> GetScheduleAsync(
        Guid userId, Guid spaceId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return Errors.UserNotFound;

        var from = ToUtc(fromUtc);
        var to = ToUtc(toUtc);
        if (to <= from)
            return Errors.InvalidTimeRange;

        var space = await _spaces.GetByIdAsync(spaceId, ct);
        if (space is null)
            return Errors.SpaceNotFound;
        if (!AccessPolicy.CanView(user, space))
            return Errors.SpaceAccessDenied;

        var bookings = await _bookings.GetBySpaceAsync(spaceId, from, to, ct);
        var dtos = bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Select(b => b.ToDto())
            .ToList();

        return Result.Success<IReadOnlyList<BookingDto>>(dtos);
    }

    public async Task<Result<IReadOnlyList<BookingDto>>> GetAllBookingsForAdminAsync(
        Guid adminId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var admin = await _users.GetByIdAsync(adminId, ct);
        if (admin is null)
            return Errors.UserNotFound;
        if (admin.Role != UserRole.Admin)
            return Errors.AdminOnly;

        var bookings = await _bookings.GetAllInRangeAsync(ToUtc(fromUtc), ToUtc(toUtc), ct);
        var dtos = bookings.Select(b => b.ToDto()).ToList();
        return Result.Success<IReadOnlyList<BookingDto>>(dtos);
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}