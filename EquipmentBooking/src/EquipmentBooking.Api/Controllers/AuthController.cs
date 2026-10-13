using EquipmentBooking.Api.Authentication;
using EquipmentBooking.Api.Contracts;
using EquipmentBooking.Api.Configuration;
using EquipmentBooking.Application.Auth;
using EquipmentBooking.Application.Auth.Contracts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EquipmentBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService auth;
    private readonly CookieSession session;
    private readonly IAntiforgery antiforgery;

    // Session handling belongs to the HTTP layer, not the application services.
    public AuthController(IAuthService auth, CookieSession session, IAntiforgery antiforgery)
    {
        this.auth = auth;
        this.session = session;
        this.antiforgery = antiforgery;
    }

    [HttpGet("csrf")]
    [AllowAnonymous]
    public ActionResult<CsrfResponse> GetCsrfToken() =>
        Ok(new CsrfResponse(antiforgery.GetAndStoreTokens(HttpContext).RequestToken!));

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetCurrent(CancellationToken cancellationToken) =>
        Ok(await auth.GetCurrentAsync(cancellationToken));

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(ConfigureRateLimiterOptions.AuthPolicy)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await auth.RegisterAsync(request, cancellationToken);
        await session.SignInAsync(user);
        return Ok(user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(ConfigureRateLimiterOptions.AuthPolicy)]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await auth.LoginAsync(request, cancellationToken);
        await session.SignInAsync(user);
        return Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await session.SignOutAsync();
        return NoContent();
    }
}
