using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClimbTrack.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<DifficultyLevel> DifficultyLevels { get; }
    DbSet<TrainingType> TrainingTypes { get; }
    DbSet<Phase> Phases { get; }
    DbSet<SessionType> SessionTypes { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<UserPlan> UserPlans { get; }
    DbSet<UserPlanWeek> UserPlanWeeks { get; }
    DbSet<UserSessionLog> UserSessionLogs { get; }
    DbSet<SessionLogMetric> SessionLogMetrics { get; }
    DbSet<PlanTemplate> PlanTemplates { get; }
    DbSet<PlanTemplateDay> PlanTemplateDays { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

