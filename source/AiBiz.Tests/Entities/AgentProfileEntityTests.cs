using AiBiz.Domain.Entities;

namespace AiBiz.Tests.Entities;

/// <summary>
/// Unit tests for AgentProfile entity — fields, FK link to Sponsor, skill join.
/// </summary>
public class AgentProfileEntityTests
{
    [Fact]
    public void AgentProfile_WithRequiredFields_CanBeCreated()
    {
        var sponsorId = Guid.NewGuid();
        var agent = new AgentProfile
        {
            SponsorId = sponsorId,
            Name = "My Agent"
        };

        Assert.Equal(sponsorId, agent.SponsorId);
        Assert.Equal("My Agent", agent.Name);
    }

    [Fact]
    public void AgentProfile_Timestamps_DefaultToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var agent = new AgentProfile { SponsorId = Guid.NewGuid(), Name = "A" };
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(agent.CreatedAt, before, after);
        Assert.InRange(agent.UpdatedAt, before, after);
    }

    [Fact]
    public void AgentProfile_OptionalFields_AreNullByDefault()
    {
        var agent = new AgentProfile { SponsorId = Guid.NewGuid(), Name = "A" };

        Assert.Null(agent.Description);
        Assert.Null(agent.AvatarUrl);
        Assert.Null(agent.ContactEndpoint);
        Assert.Null(agent.WalletAddress);
        Assert.Null(agent.Availability);
        Assert.Null(agent.Sponsor);
    }

    [Fact]
    public void AgentProfile_SponsorId_LinksToSponsor()
    {
        var sponsor = new Sponsor { Email = "s@s.com", DisplayName = "S" };
        var agent = new AgentProfile
        {
            SponsorId = sponsor.Id,
            Name = "Agent",
            Sponsor = sponsor
        };

        Assert.Equal(sponsor.Id, agent.SponsorId);
        Assert.Same(sponsor, agent.Sponsor);
    }

    [Fact]
    public void AgentProfile_AgentSkills_DefaultsToEmptyList()
    {
        var agent = new AgentProfile { SponsorId = Guid.NewGuid(), Name = "A" };
        Assert.NotNull(agent.AgentSkills);
        Assert.Empty(agent.AgentSkills);
    }

    [Fact]
    public void AgentProfile_CanHaveMultipleSkills()
    {
        var agent = new AgentProfile { SponsorId = Guid.NewGuid(), Name = "A" };
        var skill1 = new Skill { Id = 1, Name = "csharp" };
        var skill2 = new Skill { Id = 2, Name = "nlp" };

        agent.AgentSkills.Add(new AgentSkill { AgentProfileId = agent.Id, SkillId = skill1.Id, Skill = skill1 });
        agent.AgentSkills.Add(new AgentSkill { AgentProfileId = agent.Id, SkillId = skill2.Id, Skill = skill2 });

        Assert.Equal(2, agent.AgentSkills.Count);
        Assert.Contains(agent.AgentSkills, x => x.Skill!.Name == "csharp");
        Assert.Contains(agent.AgentSkills, x => x.Skill!.Name == "nlp");
    }
}
