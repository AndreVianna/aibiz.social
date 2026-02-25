using AiBiz.Domain.Entities;
using AiBiz.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiBiz.Infrastructure.Services;

/// <summary>
/// Service that enforces free-tier limits and creates agent profiles.
/// </summary>
/// <remarks>
/// Free tier = 1 agent per sponsor maximum.
/// The limit is enforced here (service layer), NOT as a DB constraint,
/// so a future premium upgrade requires no migration.
/// </remarks>
public class AgentService(AiBizDbContext db) : IAgentService
{
    private const int FreeTierAgentLimit = 1;

    public async Task<AgentProfile> CreateAgentAsync(
        Guid sponsorId,
        string name,
        string? description = null,
        CancellationToken ct = default)
    {
        var existingCount = await db.AgentProfiles
            .Where(a => a.SponsorId == sponsorId)
            .CountAsync(ct);

        if (existingCount >= FreeTierAgentLimit)
            throw new FreeTierLimitExceededException(sponsorId);

        var agent = new AgentProfile
        {
            SponsorId = sponsorId,
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.AgentProfiles.Add(agent);
        await db.SaveChangesAsync(ct);

        return agent;
    }
}
