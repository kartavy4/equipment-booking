using System.ComponentModel.DataAnnotations;

namespace EquipmentBooking.Application.Auth.Contracts;

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(200)] string Email,
    [Required, StringLength(128)] string Password);
