using EquipmentBooking.Application.Abstractions;
using EquipmentBooking.Application.Auth.Contracts;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Auth;

internal sealed class AuthService(
    IApplicationDbContext db,
    IPasswordService passwords,
    ICurrentUser currentUser) : IAuthService
{
    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new AppException(ErrorType.Conflict, "Этот email уже зарегистрирован.");
        }

        var user = new AppUser(request.Name, email);
        user.SetPasswordHash(passwords.Hash(user, request.Password));
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(user);
    }

    public async Task<UserResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

        if (user is null || !passwords.Verify(user, request.Password))
        {
            throw new AppException(ErrorType.Unauthorized, "Неверный email или пароль.");
        }

        return ToResponse(user);
    }

    public async Task<UserResponse> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var id = currentUser.Id;
        var user = await db.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken)
            ?? throw new AppException(ErrorType.Unauthorized, "Войдите в аккаунт.");

        return ToResponse(user);
    }

    private static UserResponse ToResponse(AppUser user) =>
        new(user.Id, user.Name, user.Email, user.Role);
}
