using PortlinkApp.Core.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PortlinkApp.Api.Services;

public class LmStudioHealthCheck : IHealthCheck
{
    private readonly IAIService _aiService;

    public LmStudioHealthCheck(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var available = await _aiService.IsAvailable();
        return available
            ? HealthCheckResult.Healthy("LM Studio is available")
            : HealthCheckResult.Degraded("LM Studio is not available");
    }
}
