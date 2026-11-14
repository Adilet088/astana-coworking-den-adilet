using RoomBooking.Application.Abstractions;
using RoomBooking.Application.Common;
using RoomBooking.Application.DTOs;
using RoomBooking.Application.Services;
using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;
using RoomBooking.Domain.Interfaces;

namespace RoomBooking.Tests;

public sealed class BookingAndSpaceServiceTests
{
    private readonly FixedTimeProvider _time = new(new DateTimeOffset(2026, 11, 14, 10, 0, 0, TimeSpan.Zero));
    private readonly InMemoryUserRepository _users = new();
    private readonly InMemorySpaceRepository _spaces = new();
    private readonly InMemoryBookingRepository _bookings = new();
    private readonly NoOpNotifier _notifier = new();
    private readonly ImmediateUnitOfWork _uow = new();

    private BookingService CreateBookingService() =>
        new(_bookings, _spaces, _users, _uow, _notifier, _time);

    private SpaceService CreateSpaceService() =>
        new(_spaces, _users);

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsConfirmedBooking()
    {
        var student = new User { FullName = "Deniz Akhmadjanov", Email = "student@roombooking.kz", Role = UserRole.Student };
        var space = new Space
        {
            Name = "Astana Hub",
            Location = "55/8 Mangilik El Ave [Free]",
            Category = SpaceCategory.CoworkingOpenSpace,
            Capacity = 50,
            AllowedRoles = [UserRole.Student, UserRole.Teacher, UserRole.Admin],
            IsActive = true
        };
        _users.Items.Add(student);
        _spaces.Items.Add(space);

        var service = CreateBookingService();
        var start = _time.GetUtcNow().UtcDateTime.AddHours(2);
        var result = await service.CreateAsync(student.Id, new CreateBookingRequest
        {
            SpaceId = space.Id,
            StartTimeUtc = start,
            EndTimeUtc = start.AddHours(2),
            Purpose = "Diploma work"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Confirmed, result.Value.Status);
        Assert.Single(_bookings.Items);
    }

    [Fact]
    public async Task CreateAsync_OverlappingTimeSlot_ReturnsSlotTakenError()
    {
        var student1 = new User { FullName = "Student One", Email = "s1@kz", Role = UserRole.Student };
        var student2 = new User { FullName = "Student Two", Email = "s2@kz", Role = UserRole.Student };
        var space = new Space
        {
            Name = "iHUB",
            Location = "Astana [Paid]",
            AllowedRoles = [UserRole.Student, UserRole.Admin],
            IsActive = true
        };
        _users.Items.AddRange([student1, student2]);
        _spaces.Items.Add(space);

        var start = _time.GetUtcNow().UtcDateTime.AddHours(3);
        _bookings.Items.Add(new Booking
        {
            SpaceId = space.Id,
            UserId = student1.Id,
            StartTimeUtc = start,
            EndTimeUtc = start.AddHours(2),
            Purpose = "First booking",
            Status = BookingStatus.Confirmed
        });

        var service = CreateBookingService();
        var result = await service.CreateAsync(student2.Id, new CreateBookingRequest
        {
            SpaceId = space.Id,
            StartTimeUtc = start.AddHours(1),
            EndTimeUtc = start.AddHours(3),
            Purpose = "Overlapping booking"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.SlotTaken.Code, result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_StartTimeInPast_ReturnsStartInPastError()
    {
        var student = new User { FullName = "Deniz", Email = "s@kz", Role = UserRole.Student };
        var space = new Space { Name = "Library", Location = "Astana", AllowedRoles = [UserRole.Student], IsActive = true };
        _users.Items.Add(student);
        _spaces.Items.Add(space);

        var service = CreateBookingService();
        var pastStart = _time.GetUtcNow().UtcDateTime.AddHours(-1);

        var result = await service.CreateAsync(student.Id, new CreateBookingRequest
        {
            SpaceId = space.Id,
            StartTimeUtc = pastStart,
            EndTimeUtc = pastStart.AddHours(1),
            Purpose = "Late booking"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.StartInPast.Code, result.Error.Code);
    }

    [Fact]
    public async Task CreateAsync_StudentBookingTeacherOnlyRoom_ReturnsAccessDenied()
    {
        var student = new User { FullName = "Deniz", Email = "s@kz", Role = UserRole.Student };
        var lectureHall = new Space
        {
            Name = "Room C1.2.235 (Lecture Hall)",
            Location = "Campus [Free]",
            Category = SpaceCategory.LectureHall,
            AllowedRoles = [UserRole.Teacher, UserRole.Admin],
            IsActive = true
        };
        _users.Items.Add(student);
        _spaces.Items.Add(lectureHall);

        var service = CreateBookingService();
        var start = _time.GetUtcNow().UtcDateTime.AddHours(2);

        var result = await service.CreateAsync(student.Id, new CreateBookingRequest
        {
            SpaceId = lectureHall.Id,
            StartTimeUtc = start,
            EndTimeUtc = start.AddHours(1),
            Purpose = "Trying to book restricted hall"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.SpaceAccessDenied.Code, result.Error.Code);
    }

    [Fact]
    public async Task CancelAsync_AdminCancelsStudentBooking_Succeeds()
    {
        var student = new User { FullName = "Deniz", Email = "s@kz", Role = UserRole.Student };
        var admin = new User { FullName = "Admin", Email = "admin@kz", Role = UserRole.Admin };
        var space = new Space { Name = "Yourt", Location = "Astana", AllowedRoles = [UserRole.Student, UserRole.Admin], IsActive = true };
        var start = _time.GetUtcNow().UtcDateTime.AddHours(4);
        var booking = new Booking
        {
            SpaceId = space.Id,
            Space = space,
            UserId = student.Id,
            User = student,
            StartTimeUtc = start,
            EndTimeUtc = start.AddHours(1),
            Purpose = "Study",
            Status = BookingStatus.Confirmed
        };

        _users.Items.AddRange([student, admin]);
        _spaces.Items.Add(space);
        _bookings.Items.Add(booking);

        var service = CreateBookingService();
        var result = await service.CancelAsync(admin.Id, booking.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public async Task SpaceService_CreateAsync_ByAdmin_CreatesSpaceWithEquipment()
    {
        var admin = new User { FullName = "System Admin", Email = "admin@kz", Role = UserRole.Admin };
        _users.Items.Add(admin);

        var service = CreateSpaceService();
        var result = await service.CreateAsync(admin.Id, new CreateSpaceRequest
        {
            Name = "Tech Lab 404",
            Location = "Campus, Block C [Free]",
            Category = SpaceCategory.ComputerLab,
            Capacity = 30,
            Equipment = ["Wi-Fi", "Projector"],
            AllowedRoles = [UserRole.Student, UserRole.Teacher]
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Tech Lab 404", result.Value.Name);
        Assert.Contains("Wi-Fi", result.Value.Equipment);
        Assert.Contains(UserRole.Admin, result.Value.AllowedRoles);
    }

    [Fact]
    public async Task SpaceService_CreateAsync_ByNonAdmin_ReturnsAdminOnlyError()
    {
        var student = new User { FullName = "Deniz", Email = "s@kz", Role = UserRole.Student };
        _users.Items.Add(student);

        var service = CreateSpaceService();
        var result = await service.CreateAsync(student.Id, new CreateSpaceRequest
        {
            Name = "Unauthorized Room",
            Location = "Campus",
            Capacity = 10
        });

        Assert.True(result.IsFailure);
        Assert.Equal(Errors.AdminOnly.Code, result.Error.Code);
    }

    // ---------- Lightweight In-Memory Test Doubles ----------

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public List<User> Items { get; } = new();
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(u => u.Id == id));
        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(u => u.Email == email));
        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<User>>(Items);
        public Task AddAsync(User user, CancellationToken ct = default) { Items.Add(user); return Task.CompletedTask; }
        public Task UpdateAsync(User user, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) { Items.RemoveAll(u => u.Id == id); return Task.CompletedTask; }
    }

    private sealed class InMemorySpaceRepository : ISpaceRepository
    {
        public List<Space> Items { get; } = new();
        public Task<Space?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(s => s.Id == id));
        public Task<IReadOnlyList<Space>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Space>>(Items);
        public Task<IReadOnlyList<Space>> GetByCategoryAsync(SpaceCategory category, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Space>>(Items.Where(s => s.Category == category).ToList());
        public Task<IReadOnlyList<Space>> GetAccessibleForRoleAsync(UserRole role, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Space>>(Items.Where(s => s.IsAccessibleTo(role)).ToList());
        public Task<IReadOnlyList<Equipment>> GetAllEquipmentAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Equipment>>(Array.Empty<Equipment>());
        public Task AddAsync(Space space, CancellationToken ct = default) { Items.Add(space); return Task.CompletedTask; }
        public Task AddWithEquipmentAsync(Space space, IReadOnlyCollection<string> equipmentNames, CancellationToken ct = default)
        {
            foreach (var name in equipmentNames) space.Equipment.Add(new Equipment { Name = name });
            Items.Add(space);
            return Task.CompletedTask;
        }
        public Task UpdateAsync(Space space, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) { Items.RemoveAll(s => s.Id == id); return Task.CompletedTask; }
    }

    private sealed class InMemoryBookingRepository : IBookingRepository
    {
        public List<Booking> Items { get; } = new();
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Items.FirstOrDefault(b => b.Id == id));
        public Task<IReadOnlyList<Booking>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Booking>>(Items.Where(b => b.UserId == userId).ToList());
        public Task<IReadOnlyList<Booking>> GetBySpaceAsync(Guid spaceId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Booking>>(Items.Where(b => b.SpaceId == spaceId && b.StartTimeUtc < toUtc && fromUtc < b.EndTimeUtc).ToList());
        public Task<IReadOnlyList<Booking>> GetAllInRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Booking>>(Items.Where(b => b.StartTimeUtc < toUtc && fromUtc < b.EndTimeUtc).ToList());
        public Task<bool> HasOverlapAsync(Guid spaceId, DateTime startUtc, DateTime endUtc, Guid? excludeBookingId = null, CancellationToken ct = default) =>
            Task.FromResult(Items.Any(b =>
                b.SpaceId == spaceId &&
                b.Status == BookingStatus.Confirmed &&
                b.StartTimeUtc < endUtc &&
                startUtc < b.EndTimeUtc &&
                (excludeBookingId == null || b.Id != excludeBookingId)));
        public Task AddAsync(Booking booking, CancellationToken ct = default) { Items.Add(booking); return Task.CompletedTask; }
        public Task UpdateAsync(Booking booking, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class ImmediateUnitOfWork : IUnitOfWork
    {
        public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default) => action();
    }

    private sealed class NoOpNotifier : IBookingNotifier
    {
        public Task BookingChangedAsync(BookingDto booking, CancellationToken ct = default) => Task.CompletedTask;
    }
}