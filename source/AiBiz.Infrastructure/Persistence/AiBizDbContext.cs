using AiBiz.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiBiz.Infrastructure.Persistence;

public class AiBizDbContext(DbContextOptions<AiBizDbContext> options) : DbContext(options)
{
    public DbSet<Sponsor> Sponsors => Set<Sponsor>();
    public DbSet<AgentProfile> AgentProfiles => Set<AgentProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<AgentSkill> AgentSkills => Set<AgentSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── pgvector extension ──────────────────────────────────────────────
        // The extension is enabled via a migration; this registers the types
        // so future vector columns can be added without a model change.
        modelBuilder.HasPostgresExtension("vector");

        // ── Sponsor ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Sponsor>(e =>
        {
            e.ToTable("sponsors");
            e.HasKey(s => s.Id);

            e.Property(s => s.Id)
             .HasDefaultValueSql("gen_random_uuid()");

            e.Property(s => s.Email)
             .IsRequired()
             .HasMaxLength(320);

            e.HasIndex(s => s.Email).IsUnique();

            e.Property(s => s.DisplayName)
             .IsRequired()
             .HasMaxLength(120);

            e.Property(s => s.PasswordHash)
             .HasMaxLength(512);

            e.Property(s => s.EmailVerified)
             .HasDefaultValue(false);

            e.Property(s => s.CreatedAt)
             .HasDefaultValueSql("now() at time zone 'utc'");

            // Nav: one sponsor → many agents
            e.HasMany(s => s.Agents)
             .WithOne(a => a.Sponsor)
             .HasForeignKey(a => a.SponsorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AgentProfile ─────────────────────────────────────────────────────
        modelBuilder.Entity<AgentProfile>(e =>
        {
            e.ToTable("agent_profiles");
            e.HasKey(a => a.Id);

            e.Property(a => a.Id)
             .HasDefaultValueSql("gen_random_uuid()");

            e.Property(a => a.Name)
             .IsRequired()
             .HasMaxLength(200);

            e.Property(a => a.Description)
             .HasMaxLength(2000);

            e.Property(a => a.AvatarUrl)
             .HasMaxLength(500);

            e.Property(a => a.ContactEndpoint)
             .HasMaxLength(500);

            e.Property(a => a.WalletAddress)
             .HasMaxLength(100);

            e.Property(a => a.Availability)
             .HasMaxLength(100);

            e.Property(a => a.CreatedAt)
             .HasDefaultValueSql("now() at time zone 'utc'");

            e.Property(a => a.UpdatedAt)
             .HasDefaultValueSql("now() at time zone 'utc'");
        });

        // ── Skill ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Skill>(e =>
        {
            e.ToTable("skills");
            e.HasKey(s => s.Id);

            e.Property(s => s.Id)
             .ValueGeneratedOnAdd();

            e.Property(s => s.Name)
             .IsRequired()
             .HasMaxLength(100);

            e.HasIndex(s => s.Name).IsUnique();
        });

        // ── AgentSkill (join table) ──────────────────────────────────────────
        modelBuilder.Entity<AgentSkill>(e =>
        {
            e.ToTable("agent_skills");

            e.HasKey(x => new { x.AgentProfileId, x.SkillId });

            e.HasOne(x => x.AgentProfile)
             .WithMany(a => a.AgentSkills)
             .HasForeignKey(x => x.AgentProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Skill)
             .WithMany(s => s.AgentSkills)
             .HasForeignKey(x => x.SkillId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
