namespace RoomBooking.Application.Common;

public static class Errors
{
    public static readonly Error UserNotFound = new("User.NotFound", "User not found.");
    public static readonly Error AdminOnly = new("User.AdminOnly", "This action is available to administrators only.");

    public static readonly Error SpaceNotFound = new("Space.NotFound", "Space not found.");
    public static readonly Error SpaceInactive = new("Space.Inactive", "This space is temporarily unavailable for booking.");
    public static readonly Error SpaceAccessDenied = new("Space.AccessDenied", "Your role does not have access to this space.");

    public static readonly Error BookingNotFound = new("Booking.NotFound", "Booking not found.");
    public static readonly Error BookingForbidden = new("Booking.Forbidden", "You can only manage your own bookings.");
    public static readonly Error BookingNotCancellable = new("Booking.NotCancellable", "This booking cannot be cancelled.");
    public static readonly Error SlotTaken = new("Booking.SlotTaken", "The selected time slot is already taken.");

    public static readonly Error InvalidTimeRange = new("Booking.InvalidTimeRange", "End time must be later than start time.");
    public static readonly Error StartInPast = new("Booking.StartInPast", "You cannot book a time in the past.");
    public static readonly Error PurposeRequired = new("Booking.PurposeRequired", "Please specify the purpose of the booking.");
    public static readonly Error PurposeTooLong = new("Booking.PurposeTooLong", "The booking purpose is too long.");

    public static Error DurationTooShort(TimeSpan min) =>
        new("Booking.DurationTooShort", $"Minimum booking duration is {(int)min.TotalMinutes} minutes.");

    public static Error DurationTooLong(TimeSpan max) =>
        new("Booking.DurationTooLong", $"Maximum booking duration for your role is {(int)max.TotalHours} hours.");
}