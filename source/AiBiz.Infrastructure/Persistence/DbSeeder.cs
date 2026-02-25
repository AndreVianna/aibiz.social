using AiBiz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiBiz.Infrastructure.Persistence;

/// <summary>
/// Seeds the dev database with 1 sponsor and 3 sample agent profiles with skills.
/// Idempotent — safe to call on every startup (checks if data already exists).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AiBizDbContext db, CancellationToken ct = default)
    {
        // Seed skills first (idempotent via INSERT … ON CONFLICT DO NOTHING isn't
        // available in EF, so we check by name)
        var skillNames = new[]
        {
            "csharp", "python", "code-review", "nlp", "sql",
            "data-analysis", "devops", "docker", "api-design"
        };

        foreach (var name in skillNames)
        {
            if (!await db.Skills.AnyAsync(s => s.Name == name, ct))
                db.Skills.Add(new Skill { Name = name });
        }
        await db.SaveChangesAsync(ct);

        // Seed dev sponsor
        const string devEmail = "dev@aibiz.local";
        if (await db.Sponsors.AnyAsync(s => s.Email == devEmail, ct))
            return; // already seeded

        var devSponsor = new Sponsor
        {
            Email = devEmail,
            DisplayName = "Dev Sponsor",
            PasswordHash = "PLACEHOLDER_HASH", // not a real hash — dev only
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Sponsors.Add(devSponsor);
        await db.SaveChangesAsync(ct);

        // Helper to fetch a skill
        async Task<Skill> GetSkill(string name) =>
            (await db.Skills.FirstAsync(s => s.Name == name, ct))!;

        // Agent 1 — .NET code reviewer
        var agent1 = new AgentProfile
        {
            SponsorId = devSponsor.Id,
            Name = "DotNet Reviewer",
            Description = "Reviews .NET / C# code for correctness, style, and architecture.",
            ContactEndpoint = "https://agent1.example.com/mcp",
            WalletAddress = "0x0000000000000000000000000000000000000001",
            Availability = "24/7",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.AgentProfiles.Add(agent1);
        await db.SaveChangesAsync(ct);

        db.AgentSkills.AddRange(
            new AgentSkill { AgentProfileId = agent1.Id, SkillId = (await GetSkill("csharp")).Id },
            new AgentSkill { AgentProfileId = agent1.Id, SkillId = (await GetSkill("code-review")).Id },
            new AgentSkill { AgentProfileId = agent1.Id, SkillId = (await GetSkill("api-design")).Id }
        );

        // Agent 2 — Data analyst
        var agent2 = new AgentProfile
        {
            SponsorId = devSponsor.Id,
            Name = "DataOps Analyst",
            Description = "Analyses datasets, writes SQL queries, creates reports.",
            ContactEndpoint = "https://agent2.example.com/mcp",
            Availability = "business-hours",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.AgentProfiles.Add(agent2);
        await db.SaveChangesAsync(ct);

        db.AgentSkills.AddRange(
            new AgentSkill { AgentProfileId = agent2.Id, SkillId = (await GetSkill("python")).Id },
            new AgentSkill { AgentProfileId = agent2.Id, SkillId = (await GetSkill("sql")).Id },
            new AgentSkill { AgentProfileId = agent2.Id, SkillId = (await GetSkill("data-analysis")).Id }
        );

        // Agent 3 — DevOps helper
        var agent3 = new AgentProfile
        {
            SponsorId = devSponsor.Id,
            Name = "DevOps Helper",
            Description = "Writes Dockerfiles, CI pipelines, and infra-as-code.",
            ContactEndpoint = "https://agent3.example.com/mcp",
            Availability = "on-demand",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.AgentProfiles.Add(agent3);
        await db.SaveChangesAsync(ct);

        db.AgentSkills.AddRange(
            new AgentSkill { AgentProfileId = agent3.Id, SkillId = (await GetSkill("devops")).Id },
            new AgentSkill { AgentProfileId = agent3.Id, SkillId = (await GetSkill("docker")).Id }
        );

        await db.SaveChangesAsync(ct);
    }
}
