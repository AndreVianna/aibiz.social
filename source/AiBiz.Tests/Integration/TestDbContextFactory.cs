using AiBiz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiBiz.Tests.Integration;

/// <summary>
/// Creates a clean AiBizDbContext pointed at the aibiz_test database.
/// Each test should call CreateContext() to get a fresh context.
/// </summary>
public static class TestDbContextFactory
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=aibiz_test;Username=aibiz;Password=aibiz_dev_password";

    public static AiBizDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AiBizDbContext>()
            .UseNpgsql(TestConnectionString, npgsql => npgsql.UseVector())
            .Options;

        return new AiBizDbContext(options);
    }
}
