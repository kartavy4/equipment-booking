using EquipmentBooking.Api.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EquipmentBooking.Api.Configuration;

internal sealed class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        options.Filters.Add<ValidateCsrfTokenFilter>();
        options.Filters.Add<ExpectedExceptionFilter>();
    }
}
