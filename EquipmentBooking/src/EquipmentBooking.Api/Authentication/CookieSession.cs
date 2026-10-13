using System.Security.Claims;
using EquipmentBooking.Application.Auth.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EquipmentBooking.Api.Authentication;

public sealed class CookieSession(IHttpContextAccessor accessor)
{
    public Task SignInAsync(UserResponse user)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    public Task SignOutAsync() =>
        Context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    private HttpContext Context =>
        accessor.HttpContext ?? throw new InvalidOperationException("An HTTP request is required.");
}
