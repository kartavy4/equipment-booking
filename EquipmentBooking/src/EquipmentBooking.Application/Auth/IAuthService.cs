using EquipmentBooking.Application.Auth.Contracts;

namespace EquipmentBooking.Application.Auth;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<UserResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<UserResponse> GetCurrentAsync(CancellationToken cancellationToken);
}
