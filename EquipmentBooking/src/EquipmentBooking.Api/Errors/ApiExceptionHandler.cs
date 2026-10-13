using Microsoft.AspNetCore.Diagnostics;

namespace EquipmentBooking.Api.Errors;

internal sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        const int status = StatusCodes.Status500InternalServerError;
        const string message = "Произошла внутренняя ошибка. Попробуйте позже.";
        logger.LogError(exception, "Unhandled API error. TraceId: {TraceId}", context.TraceIdentifier);

        context.Response.StatusCode = status;
        var problem = ApiProblem.Create(status, message, context);

        if (!await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem
        }))
        {
            await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        }

        return true;
    }
}
