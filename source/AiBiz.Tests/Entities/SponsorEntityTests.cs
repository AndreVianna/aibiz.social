using AiBiz.Domain.Entities;

namespace AiBiz.Tests.Entities;

/// <summary>
/// Unit tests for the Sponsor entity — required fields, defaults, and relationships.
/// These are pure object tests; no database required.
/// </summary>
public class SponsorEntityTests
{
    [Fact]
    public void Sponsor_WithRequiredFields_CanBeCreated()
    {
        var sponsor = new Sponsor
        {
            Email = "test@example.com",
            DisplayName = "Test Sponsor"
        };

        Assert.Equal("test@example.com", sponsor.Email);
        Assert.Equal("Test Sponsor", sponsor.DisplayName);
    }

    [Fact]
    public void Sponsor_CreatedAt_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var sponsor = new Sponsor { Email = "a@b.com", DisplayName = "X" };
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(sponsor.CreatedAt, before, after);
    }

    [Fact]
    public void Sponsor_EmailVerified_DefaultsToFalse()
    {
        var sponsor = new Sponsor { Email = "a@b.com", DisplayName = "X" };
        Assert.False(sponsor.EmailVerified);
    }

    [Fact]
    public void Sponsor_Agents_DefaultsToEmptyList()
    {
        var sponsor = new Sponsor { Email = "a@b.com", DisplayName = "X" };
        Assert.NotNull(sponsor.Agents);
        Assert.Empty(sponsor.Agents);
    }

    [Fact]
    public void Sponsor_CanHaveMultipleAgents()
    {
        var sponsor = new Sponsor { Email = "a@b.com", DisplayName = "X" };
        sponsor.Agents.Add(new AgentProfile { SponsorId = sponsor.Id, Name = "Agent A" });
        sponsor.Agents.Add(new AgentProfile { SponsorId = sponsor.Id, Name = "Agent B" });

        Assert.Equal(2, sponsor.Agents.Count);
    }

    [Fact]
    public void Sponsor_PasswordHash_IsNullableByDefault()
    {
        var sponsor = new Sponsor { Email = "a@b.com", DisplayName = "X" };
        Assert.Null(sponsor.PasswordHash);
    }
}
