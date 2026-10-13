using System.ComponentModel.DataAnnotations;

namespace EquipmentBooking.Infrastructure.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    [Required, EmailAddress, StringLength(200)]
    public string AdminEmail { get; init; } = "admin@equipment.local";

    [StringLength(128, MinimumLength = 10)]
    public string? AdminPassword { get; init; }
}
