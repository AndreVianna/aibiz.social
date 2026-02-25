using AiBiz.Infrastructure.Persistence;
using AiBiz.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Reqnroll.BoDi;

namespace AiBiz.IntegrationTests.Support;

/// <summary>
/// Sets up a real AiBizDbContext (test database) for scenarios tagged @freetier.
/// </summary>
[Binding]
public sealed class FreeTierHooks(IObjectContainer objectContainer)
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=aibiz_test;Username=aibiz;Password=aibiz_dev_password";

    private AiBizDbContext? _db;

    [BeforeScenario("freetier")]
    public void BeforeFreeTierScenario()
    {
        var options = new DbContextOptionsBuilder<AiBizDbContext>()
            .UseNpgsql(TestConnectionString, npgsql => npgsql.UseVector())
            .Options;

        _db = new AiBizDbContext(options);
        var agentService = new AgentService(_db);
        var ctx = new FreeTierTestContext();

        // Register services so step definitions can inject them
        objectContainer.RegisterInstanceAs(_db);
        objectContainer.RegisterInstanceAs<IAgentService>(agentService);
        objectContainer.RegisterInstanceAs(ctx);
    }

    [AfterScenario("freetier")]
    public async Task AfterFreeTierScenario()
    {
        if (_db is null) return;

        try
        {
            // Cleanup — the context is retrieved from the container
            var ctx = objectContainer.Resolve<FreeTierTestContext>();
            if (ctx.TestSponsorId != Guid.Empty)
            {
                var sponsor = await _db.Sponsors.FindAsync(ctx.TestSponsorId);
                if (sponsor is not null)
                {
                    _db.Sponsors.Remove(sponsor);
                    await _db.SaveChangesAsync();
                }
            }
        }
        finally
        {
            await _db.DisposeAsync();
        }
    }
}
