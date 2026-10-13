using EquipmentBooking.Domain.Users;

namespace EquipmentBooking.Application.Abstractions;

public interface IPasswordService
{
    string Hash(AppUser user, string password);
    bool Verify(AppUser user, string password);
}
