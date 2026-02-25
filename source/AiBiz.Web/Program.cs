using System.Text.Json;
using AiBiz.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// ── Razor Pages ───────────────────────────────────────────────────────────────
builder.Services.AddRazorPages();

// ── Infrastructure (EF Core + PostgreSQL + services) ─────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Health Checks ─────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var hcBuilder = builder.Services.AddHealthChecks();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    hcBuilder.AddNpgSql(
        connectionString,
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "ready"]);
}
else
{
    // No connection string configured — surface as Degraded so the app still
    // starts in development / test without a real database.
    hcBuilder.AddCheck(
        "database",
        () => HealthCheckResult.Degraded("No connection string configured."));
}

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Health endpoint: /health → JSON with status + per-check details
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => e.Value.Status.ToString())
        };

        await ctx.Response.WriteAsync(
            JsonSerializer.Serialize(payload));
    }
});

app.MapGet("/", () => "Hello World!");
app.MapRazorPages();

app.Run();

// Required so WebApplicationFactory<Program> can reference this assembly
// from AiBiz.Tests and AiBiz.IntegrationTests.
public partial class Program { }
