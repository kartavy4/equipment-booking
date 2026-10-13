using System.Security.Claims;
using EquipmentBooking.Application.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Domain.Users;

namespace EquipmentBooking.Api.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id
    {
        get
        {
            var id = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(id, out var userId))
            {
                throw new AppException(ErrorType.Unauthorized, "Войдите в аккаунт.");
            }

            return userId;
        }
    }

    public bool IsAdmin => accessor.HttpContext?.User.IsInRole(Roles.Admin) == true;
}
