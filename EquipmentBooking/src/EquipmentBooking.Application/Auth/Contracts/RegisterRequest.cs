using System.ComponentModel.DataAnnotations;

namespace EquipmentBooking.Application.Auth.Contracts;

public sealed record RegisterRequest(
    [Required, StringLength(80, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(200)] string Email,
    [Required, StringLength(128, MinimumLength = 10)] string Password);
