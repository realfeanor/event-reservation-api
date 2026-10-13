using Microsoft.Extensions.Diagnostics.HealthChecks;
using Persistence.Contexts;

namespace WebAPI.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly BaseDbContext _context;

    public DatabaseHealthCheck(BaseDbContext context) => _context = context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("The database is reachable.")
                : HealthCheckResult.Unhealthy("The database is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("The database readiness check failed.", exception);
        }
    }
}
