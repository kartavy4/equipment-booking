using System.ComponentModel.DataAnnotations;

namespace EquipmentBooking.Application.Bookings.Contracts;

public sealed record CreateBooking(
    Guid EquipmentId,
    [Required] DateTimeOffset StartsAt,
    [Required] DateTimeOffset EndsAt);
