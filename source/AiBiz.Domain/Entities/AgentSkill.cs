namespace AiBiz.Domain.Entities;

/// <summary>
/// Many-to-many join: an agent has zero or more skills.
/// </summary>
public class AgentSkill
{
    public Guid AgentProfileId { get; set; }
    public AgentProfile? AgentProfile { get; set; }

    public int SkillId { get; set; }
    public Skill? Skill { get; set; }
}
