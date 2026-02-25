using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AiBiz.Tests;

/// <summary>
/// Unit-level tests for the /health endpoint.
/// Uses WebApplicationFactory with mocked health checks — no real database required.
/// </summary>
public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_Returns200_WhenDatabaseIsHealthy()
    {
        // Arrange — replace all registered health checks with an always-healthy stub
        var client = _factory
            .WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                    services.Configure<HealthCheckServiceOptions>(options =>
                    {
                        options.Registrations.Clear();
                        options.Registrations.Add(CreateRegistration(
                            HealthCheckResult.Healthy("Stub: database OK")));
                    })))
            .CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
        body.Should().Contain("database");
    }

    [Fact]
    public async Task HealthEndpoint_Returns503_WhenDatabaseIsUnhealthy()
    {
        // Arrange — replace all registered health checks with an always-unhealthy stub
        var client = _factory
            .WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                    services.Configure<HealthCheckServiceOptions>(options =>
                    {
                        options.Registrations.Clear();
                        options.Registrations.Add(CreateRegistration(
                            HealthCheckResult.Unhealthy("Stub: database unreachable")));
                    })))
            .CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Unhealthy");
        body.Should().Contain("database");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static HealthCheckRegistration CreateRegistration(HealthCheckResult result) =>
        new(
            name: "database",
            factory: _ => new StaticHealthCheck(result),
            failureStatus: HealthStatus.Unhealthy,
            tags: ["db"]);

    private sealed class StaticHealthCheck(HealthCheckResult result) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }
}
