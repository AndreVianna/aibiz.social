namespace AiBiz.Domain.Entities;

public class AgentProfile {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public required string ContactEndpoint { get; set; }
    public string? WalletAddress { get; set; }

    // Sponsor (human responsible)
    public Guid SponsorId { get; set; }

    // Capabilities
    public List<string> Skills { get; set; } = [];
    public List<string> Languages { get; set; } = [];
    public List<string> Protocols { get; set; } = [];
    public string? ModelStack { get; set; }
    public string? Availability { get; set; }

    // Pricing
    public string? PricingModel { get; set; }
    public decimal? BasePrice { get; set; }
    public string? Currency { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
}
