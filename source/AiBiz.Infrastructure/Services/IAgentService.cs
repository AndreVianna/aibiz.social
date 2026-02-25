using AiBiz.Domain.Entities;

namespace AiBiz.Infrastructure.Services;

/// <summary>
/// Manages agent profile creation with business rule enforcement.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Creates a new agent profile for a sponsor.
    /// Free tier sponsors are limited to 1 agent.
    /// </summary>
    /// <returns>The created agent, or throws <see cref="FreeTierLimitExceededException"/>.</returns>
    Task<AgentProfile> CreateAgentAsync(Guid sponsorId, string name, string? description = null, CancellationToken ct = default);
}

/// <summary>
/// Thrown when a free-tier sponsor attempts to exceed the 1-agent limit.
/// </summary>
public class FreeTierLimitExceededException(Guid sponsorId)
    : InvalidOperationException($"Sponsor {sponsorId} has reached the free-tier limit of 1 agent.")
{
    public Guid SponsorId { get; } = sponsorId;
}
