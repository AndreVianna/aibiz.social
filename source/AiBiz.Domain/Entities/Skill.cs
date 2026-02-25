namespace AiBiz.Domain.Entities;

/// <summary>
/// A normalized skill tag (e.g., "csharp", "code-review", "nlp").
/// </summary>
public class Skill
{
    public int Id { get; set; }

    /// <summary>
    /// Normalized tag name (lowercase, kebab-case). Unique.
    /// </summary>
    public required string Name { get; set; }

    public List<AgentSkill> AgentSkills { get; set; } = [];
}
