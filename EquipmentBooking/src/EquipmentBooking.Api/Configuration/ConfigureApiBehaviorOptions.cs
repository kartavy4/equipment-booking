using EquipmentBooking.Api.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EquipmentBooking.Api.Configuration;

internal sealed class ConfigureApiBehaviorOptions : IConfigureOptions<ApiBehaviorOptions>
{
    public void Configure(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = ApiProblem.Create(
                StatusCodes.Status400BadRequest,
                "Проверьте обязательные поля и формат данных.",
                context.HttpContext);

            problem.Extensions["errors"] = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "Некорректное значение."
                            : error.ErrorMessage)
                        .ToArray());

            return new BadRequestObjectResult(problem);
        };
    }
}
