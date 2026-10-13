using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Users;

public sealed class AppUser
{
    private AppUser() { }

    public AppUser(string name, string email, string role = Roles.Member)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();

        if (Name.Length is < 2 or > 80)
        {
            throw new DomainRuleException("Имя должно содержать от 2 до 80 символов.");
        }

        Id = Guid.NewGuid();
        Role = role;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = Roles.Member;

    public void SetPasswordHash(string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        PasswordHash = hash;
    }
}
