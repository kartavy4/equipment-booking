using System.ComponentModel.DataAnnotations;

namespace EquipmentBooking.Application.Equipment.Contracts;

public sealed record EquipmentRequest(
    [Required, StringLength(120)] string Name,
    [Required, StringLength(60)] string Category,
    [Required(AllowEmptyStrings = true), StringLength(1000)] string Description,
    bool IsActive = true);
