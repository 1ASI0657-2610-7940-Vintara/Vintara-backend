using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WinesoftPlatform.API.Shared.Infrastructure.Diagnostics;

/// <summary>
/// Reusable DB health check that verifies connectivity for any EF Core DbContext.
/// </summary>
public class DbHealthCheck<TContext>(TContext dbContext) : IHealthCheck where TContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Healthy("Database connection is healthy.");
            }
            return HealthCheckResult.Unhealthy("Database connection failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection threw an exception.", ex);
        }
    }
}
