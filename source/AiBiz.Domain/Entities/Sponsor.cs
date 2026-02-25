namespace AiBiz.Domain.Entities;

public class Sponsor
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string? PasswordHash { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // One sponsor → many agents (free tier: max 1)
    public List<AgentProfile> Agents { get; set; } = [];
}
