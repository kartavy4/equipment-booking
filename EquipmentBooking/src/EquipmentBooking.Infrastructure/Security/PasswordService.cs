using EquipmentBooking.Application.Abstractions;
using EquipmentBooking.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace EquipmentBooking.Infrastructure.Security;

internal sealed class PasswordService(IPasswordHasher<AppUser> hasher) : IPasswordService
{
    public string Hash(AppUser user, string password) => hasher.HashPassword(user, password);

    public bool Verify(AppUser user, string password) =>
        hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
