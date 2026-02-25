using AiBiz.Infrastructure.Persistence;
using AiBiz.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiBiz.Infrastructure;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers EF Core + PostgreSQL, the DbContext, and application services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AiBizDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.UseVector()));

        services.AddScoped<IAgentService, AgentService>();

        return services;
    }

    /// <summary>
    /// Applies pending migrations and optionally seeds development data.
    /// Call from Program.cs on startup in development/staging.
    /// </summary>
    public static async Task MigrateAndSeedAsync(
        this IServiceProvider serviceProvider,
        bool seed = true,
        CancellationToken ct = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AiBizDbContext>();

        await db.Database.MigrateAsync(ct);

        if (seed)
            await DbSeeder.SeedAsync(db, ct);
    }
}
