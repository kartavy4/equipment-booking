using EquipmentBooking.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EquipmentBooking.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(HealthCheckService healthChecks) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        return report.Status == HealthStatus.Healthy
            ? Ok(new HealthResponse("healthy"))
            : StatusCode(StatusCodes.Status503ServiceUnavailable, new HealthResponse("unhealthy"));
    }
}
