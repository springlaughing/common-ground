using CommonGround.Modules.Audit.Entities;
using CommonGround.Modules.Comparisons.Entities;
using CommonGround.Modules.Questionnaires.Entities;
using CommonGround.Modules.Reporting.Entities;
using CommonGround.Modules.Responses.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Api.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Questionnaires
    public DbSet<QuestionnaireVersion> QuestionnaireVersions => Set<QuestionnaireVersion>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<DimensionWeight> DimensionWeights => Set<DimensionWeight>();
    public DbSet<DimensionMaxScore> DimensionMaxScores => Set<DimensionMaxScore>();

    // Responses
    public DbSet<ResponseSet> ResponseSets => Set<ResponseSet>();
    public DbSet<Answer> Answers => Set<Answer>();

    // Reporting
    public DbSet<DimensionScore> DimensionScores => Set<DimensionScore>();
    public DbSet<InsightSnippet> InsightSnippets => Set<InsightSnippet>();
    public DbSet<DimensionGroup> DimensionGroups => Set<DimensionGroup>();
    public DbSet<DimensionGroupMembership> DimensionGroupMemberships => Set<DimensionGroupMembership>();

    // Comparisons
    public DbSet<ComparisonSession> ComparisonSessions => Set<ComparisonSession>();
    public DbSet<ComparisonParticipant> ComparisonParticipants => Set<ComparisonParticipant>();

    // Audit
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
