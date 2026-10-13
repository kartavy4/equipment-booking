namespace EquipmentBooking.Application.Bookings.Contracts;

public sealed record BookingResponse(
    Guid Id,
    Guid EquipmentId,
    string EquipmentName,
    string UserName,
    DateTime StartsAt,
    DateTime EndsAt,
    bool IsCancelled);
