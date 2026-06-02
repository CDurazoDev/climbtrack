using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Infrastructure.Persistence;

public class ClimbTrackDbContext : DbContext, IApplicationDbContext
{
    public ClimbTrackDbContext(DbContextOptions<ClimbTrackDbContext> options) : base(options) { }

    public DbSet<TrainingType> TrainingTypes => Set<TrainingType>();
    public DbSet<DifficultyLevel> DifficultyLevels => Set<DifficultyLevel>();
    public DbSet<EnergySystem> EnergySystems => Set<EnergySystem>();
    public DbSet<Phase> Phases => Set<Phase>();
    public DbSet<SessionType> SessionTypes => Set<SessionType>();
    public DbSet<SessionBlock> SessionBlocks => Set<SessionBlock>();
    public DbSet<SessionBlockItem> SessionBlockItems => Set<SessionBlockItem>();

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserPlan> UserPlans => Set<UserPlan>();
    public DbSet<UserPlanWeek> UserPlanWeeks => Set<UserPlanWeek>();
    public DbSet<UserSessionLog> UserSessionLogs => Set<UserSessionLog>();
    public DbSet<SessionLogMetric> SessionLogMetrics => Set<SessionLogMetric>();
    public DbSet<PlanTemplate> PlanTemplates => Set<PlanTemplate>();
    public DbSet<PlanTemplateDay> PlanTemplateDays => Set<PlanTemplateDay>();
    public DbSet<UserCustomSession> UserCustomSessions => Set<UserCustomSession>();
    public DbSet<UserCustomSessionBlock> UserCustomSessionBlocks => Set<UserCustomSessionBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClimbTrackDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

