using RoomBooking.Application.DTOs;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Web.State;

public enum LocationFilter { All, Campus, Astana }

public sealed class BookingStateContainer
{
    private List<BookingDto> _bookings = [];

    public event Action? OnChange;

    public DateOnly SelectedDate { get; private set; } = DateOnly.FromDateTime(DateTime.Today);
    public LocationFilter Location { get; private set; } = LocationFilter.All;
    public SpaceCategory? Category { get; private set; }
    public int MinCapacity { get; private set; }
    public IReadOnlyList<BookingDto> Bookings => _bookings;

    public void SetDate(DateOnly date) { SelectedDate = date; Notify(); }
    public void SetLocation(LocationFilter location) { Location = location; Notify(); }
    public void SetCategory(SpaceCategory? category) { Category = category; Notify(); }
    public void SetMinCapacity(int minCapacity) { MinCapacity = Math.Max(0, minCapacity); Notify(); }

    public void ResetFilters()
    {
        Location = LocationFilter.All;
        Category = null;
        MinCapacity = 0;
        Notify();
    }

    public void SetBookings(IEnumerable<BookingDto> bookings)
    {
        _bookings = bookings.ToList();
        Notify();
    }

    /// <summary>Inserts or replaces a booking received from the hub.</summary>
    public void ApplyBookingChange(BookingDto booking)
    {
        var index = _bookings.FindIndex(b => b.Id == booking.Id);
        if (index >= 0) _bookings[index] = booking;
        else _bookings.Add(booking);
        Notify();
    }

    public bool Matches(SpaceDto space)
    {
        if (Category is not null && space.Category != Category) return false;
        if (space.Capacity < MinCapacity) return false;

        var isCampus = space.Location.Contains("Campus", StringComparison.OrdinalIgnoreCase);
        return Location switch
        {
            LocationFilter.Campus => isCampus,
            LocationFilter.Astana => !isCampus,
            _ => true
        };
    }

    private void Notify() => OnChange?.Invoke();
}