using AiBiz.Domain.Entities;
using AiBiz.Infrastructure.Persistence;
using AiBiz.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AiBiz.Tests.Integration;

/// <summary>
/// Integration tests that exercise the real PostgreSQL database (aibiz_test).
/// Each test creates its own unique data and cleans up after itself.
/// </summary>
[Trait("Category", "Integration")]
public class DbRoundTripTests : IAsyncLifetime
{
    private AiBizDbContext _db = null!;
    private Guid _testSponsorId;

    public async Task InitializeAsync()
    {
        _db = TestDbContextFactory.CreateContext();
        // Quick connectivity check — skip test if DB is unavailable
        await _db.Database.OpenConnectionAsync();
    }

    public async Task DisposeAsync()
    {
        // Cleanup this test's data (cascade deletes agents + skills)
        var sponsor = await _db.Sponsors.FindAsync(_testSponsorId);
        if (sponsor is not null)
        {
            _db.Sponsors.Remove(sponsor);
            await _db.SaveChangesAsync();
        }
        await _db.DisposeAsync();
    }

    // ── Test 1: Create sponsor and read it back ─────────────────────────────

    [Fact]
    public async Task CreateSponsor_ShouldPersistAndBeQueryable()
    {
        // Arrange
        var email = $"test-{Guid.NewGuid():N}@roundtrip.local";
        var sponsor = new Sponsor
        {
            Email = email,
            DisplayName = "Round-Trip Sponsor",
            EmailVerified = true
        };

        // Act
        _db.Sponsors.Add(sponsor);
        await _db.SaveChangesAsync();
        _testSponsorId = sponsor.Id;

        // Clear tracker, reload from DB
        _db.ChangeTracker.Clear();
        var loaded = await _db.Sponsors.FirstOrDefaultAsync(s => s.Email == email);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.DisplayName.Should().Be("Round-Trip Sponsor");
        loaded.EmailVerified.Should().BeTrue();
        loaded.Id.Should().NotBe(Guid.Empty);
        loaded.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── Test 2: Create agent under sponsor, verify FK ────────────────────────

    [Fact]
    public async Task CreateAgent_UnderSponsor_ForeignKeyShouldBeVerified()
    {
        // Arrange — create a sponsor
        var sponsor = new Sponsor
        {
            Email = $"fk-{Guid.NewGuid():N}@roundtrip.local",
            DisplayName = "FK Sponsor"
        };
        _db.Sponsors.Add(sponsor);
        await _db.SaveChangesAsync();
        _testSponsorId = sponsor.Id;

        // Act — create an agent linked to this sponsor
        var agent = new AgentProfile
        {
            SponsorId = sponsor.Id,
            Name = "FK Agent",
            ContactEndpoint = "https://fk-agent.local/mcp",
            Availability = "24/7"
        };
        _db.AgentProfiles.Add(agent);
        await _db.SaveChangesAsync();

        // Clear tracker, reload with sponsor navigation
        _db.ChangeTracker.Clear();
        var loaded = await _db.AgentProfiles
            .Include(a => a.Sponsor)
            .FirstOrDefaultAsync(a => a.Id == agent.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.SponsorId.Should().Be(sponsor.Id);
        loaded.Sponsor.Should().NotBeNull();
        loaded.Sponsor!.Email.Should().Contain("fk-");
        loaded.ContactEndpoint.Should().Be("https://fk-agent.local/mcp");
    }

    // ── Test 3: Query agents by skill ───────────────────────────────────────

    [Fact]
    public async Task QueryBySkill_ShouldReturnCorrectAgents()
    {
        // Arrange — sponsor
        var sponsor = new Sponsor
        {
            Email = $"skill-{Guid.NewGuid():N}@roundtrip.local",
            DisplayName = "Skill Sponsor"
        };
        _db.Sponsors.Add(sponsor);
        await _db.SaveChangesAsync();
        _testSponsorId = sponsor.Id;

        // Unique skill name to avoid conflicts with other tests / seed data
        var uniqueSkillName = $"test-skill-{Guid.NewGuid():N}";
        var skill = new Skill { Name = uniqueSkillName };
        _db.Skills.Add(skill);
        await _db.SaveChangesAsync();

        // Agent 1 — has the skill
        var agentWithSkill = new AgentProfile
        {
            SponsorId = sponsor.Id,
            Name = "Skilled Agent"
        };
        _db.AgentProfiles.Add(agentWithSkill);
        await _db.SaveChangesAsync();

        _db.AgentSkills.Add(new AgentSkill
        {
            AgentProfileId = agentWithSkill.Id,
            SkillId = skill.Id
        });

        // Agent 2 — no skill
        var agentWithoutSkill = new AgentProfile
        {
            SponsorId = sponsor.Id,
            Name = "Generic Agent"
        };
        _db.AgentProfiles.Add(agentWithoutSkill);
        await _db.SaveChangesAsync();

        await _db.SaveChangesAsync();

        // Act — query by skill name
        _db.ChangeTracker.Clear();
        var result = await _db.AgentProfiles
            .Where(a => a.SponsorId == sponsor.Id &&
                        a.AgentSkills.Any(s => s.Skill!.Name == uniqueSkillName))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Skilled Agent");

        // Cleanup the skill (sponsor cascade deletes agents+agent_skills)
        _db.ChangeTracker.Clear();
        var skillToDelete = await _db.Skills.FindAsync(skill.Id);
        if (skillToDelete is not null)
        {
            _db.Skills.Remove(skillToDelete);
            await _db.SaveChangesAsync();
        }
    }
}
