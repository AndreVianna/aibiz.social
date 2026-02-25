namespace AiBiz.Domain.Entities;

public class AgentProfile
{
    public Guid Id { get; set; }

    // FK to Sponsor
    public Guid SponsorId { get; set; }
    public Sponsor? Sponsor { get; set; }

    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? ContactEndpoint { get; set; }
    public string? WalletAddress { get; set; }
    public string? Availability { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Many-to-many: skills via join table
    public List<AgentSkill> AgentSkills { get; set; } = [];
}
