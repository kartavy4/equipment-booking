using EquipmentBooking.Api.Authentication;
using EquipmentBooking.Api.Configuration;
using EquipmentBooking.Api.Errors;
using EquipmentBooking.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace EquipmentBooking.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.ConfigureOptions<ConfigureMvcOptions>();
        services.ConfigureOptions<ConfigureApiBehaviorOptions>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<CookieSession>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieOptions>();
        services.AddAuthorization();
        services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
        services.AddRateLimiter(_ => { });
        services.ConfigureOptions<ConfigureRateLimiterOptions>();

        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }
}
