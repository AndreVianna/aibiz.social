using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AiBiz.IntegrationTests.Support;

/// <summary>
/// Shared WebApplicationFactory fixture for integration tests.
/// Replaces the PostgreSQL health check with an in-memory stub so tests
/// can run without a real database (no Docker required in unit CI).
/// </summary>
public class WebAppFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace any real health checks with a test stub
            services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Clear();
                options.Registrations.Add(new HealthCheckRegistration(
                    name: "database",
                    factory: _ => new StaticHealthCheck(HealthCheckResult.Healthy("Test: stub database")),
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["db", "ready"]));
            });
        });

        builder.UseEnvironment("Test");
    }

    /// <summary>Simple health check that always returns a predetermined result.</summary>
    private sealed class StaticHealthCheck(HealthCheckResult result) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }
}
