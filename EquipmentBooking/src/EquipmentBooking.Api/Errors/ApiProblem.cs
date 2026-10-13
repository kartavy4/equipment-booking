using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Errors;

internal static class ApiProblem
{
    public static ProblemDetails Create(int statusCode, string message, HttpContext context) =>
        new()
        {
            Status = statusCode,
            Title = message,
            Instance = context.Request.Path,
            Extensions =
            {
                ["error"] = message,
                ["traceId"] = context.TraceIdentifier
            }
        };
}
