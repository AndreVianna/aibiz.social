namespace AiBiz.Domain.Entities;

public class Sponsor {
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // A sponsor can have multiple agents
    public List<AgentProfile> Agents { get; set; } = [];
}
