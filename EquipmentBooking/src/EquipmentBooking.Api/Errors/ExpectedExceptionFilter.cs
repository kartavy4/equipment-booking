using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EquipmentBooking.Api.Errors;

internal sealed class ExpectedExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var status = context.Exception switch
        {
            DomainRuleException => StatusCodes.Status400BadRequest,
            AppException { Type: ErrorType.NotFound } => StatusCodes.Status404NotFound,
            AppException { Type: ErrorType.Conflict } => StatusCodes.Status409Conflict,
            AppException { Type: ErrorType.Unauthorized } => StatusCodes.Status401Unauthorized,
            _ => (int?)null
        };

        if (status is null)
        {
            return;
        }

        context.Result = new ObjectResult(ApiProblem.Create(
            status.Value, context.Exception.Message, context.HttpContext))
        {
            StatusCode = status
        };
        context.ExceptionHandled = true;
    }
}
