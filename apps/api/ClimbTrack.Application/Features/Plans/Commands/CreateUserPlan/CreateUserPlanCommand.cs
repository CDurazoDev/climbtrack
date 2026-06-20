using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Entities;
using ClimbTrack.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Plans.Commands.CreateUserPlan;

public record CreateUserPlanCommand(
    string Name,
    int TrainingTypeId,
    int DifficultyLevelId,
    DateTime StartDate,
    int? PlanTemplateId) : IRequest<Result<UserPlanDto>>;

public class CreateUserPlanCommandValidator : AbstractValidator<CreateUserPlanCommand>
{
    public CreateUserPlanCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.TrainingTypeId).GreaterThan(0);
        RuleFor(command => command.DifficultyLevelId).GreaterThan(0);
        RuleFor(command => command.StartDate).NotEmpty();
        RuleFor(command => command.PlanTemplateId)
            .GreaterThan(0)
            .When(command => command.PlanTemplateId.HasValue);
    }
}

public class CreateUserPlanCommandHandler : IRequestHandler<CreateUserPlanCommand, Result<UserPlanDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateUserPlanCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<UserPlanDto>> Handle(CreateUserPlanCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<UserPlanDto>("Authentication is required.");
        }

        var userId = _currentUser.UserId.Value;

        var trainingType = await _db.TrainingTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == request.TrainingTypeId, ct);

        if (trainingType is null)
        {
            return Result.Failure<UserPlanDto>("Training type not found.");
        }

        var difficultyLevel = await _db.DifficultyLevels
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == request.DifficultyLevelId, ct);

        if (difficultyLevel is null)
        {
            return Result.Failure<UserPlanDto>("Difficulty level not found.");
        }

        PlanTemplate? planTemplate = null;
        if (request.PlanTemplateId.HasValue)
        {
            planTemplate = await _db.PlanTemplates
                .Include(template => template.Days)
                .FirstOrDefaultAsync(template => template.Id == request.PlanTemplateId.Value, ct);

            if (planTemplate is null)
            {
                return Result.Failure<UserPlanDto>("Plan template not found.");
            }

            var canUseTemplate =
                planTemplate.Source == "system" ||
                planTemplate.IsPublic ||
                planTemplate.OwnerUserId == userId;

            if (!canUseTemplate)
            {
                return Result.Failure<UserPlanDto>("The selected plan template is not available for this user.");
            }

            var trainingDays = planTemplate.Days.Count(day => !day.IsRest);
            if (trainingDays > difficultyLevel.MaxDaysWeek)
            {
                return Result.Failure<UserPlanDto>(
                    $"The selected template exceeds the allowed {difficultyLevel.MaxDaysWeek} training days per week for this level.");
            }
        }

        var normalizedStartDate = request.StartDate.Date;
        var previousPlanEndDate = normalizedStartDate.AddDays(-1);

        await _db.UserPlans
            .Where(plan => plan.UserId == userId && plan.IsActive)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(plan => plan.IsActive, false)
                    .SetProperty(plan => plan.EndDate, previousPlanEndDate),
                ct);

        var phases = await _db.Phases
            .AsNoTracking()
            .OrderBy(phase => phase.SortOrder)
            .ToListAsync(ct);

        if (phases.Count == 0)
        {
            return Result.Failure<UserPlanDto>("The phases catalog is not initialized.");
        }

        var plan = new UserPlan(
            userId,
            request.Name.Trim(),
            request.TrainingTypeId,
            request.DifficultyLevelId,
            normalizedStartDate);

        _db.UserPlans.Add(plan);
        await _db.SaveChangesAsync(ct);

        foreach (var week in PlanPhaseHelper.BuildWeeks(phases, normalizedStartDate, request.PlanTemplateId))
        {
            _db.UserPlanWeeks.Add(new UserPlanWeek(
                plan.Id,
                week.WeekNumber,
                week.PhaseId,
                week.StartDate,
                week.IsDeload,
                0,
                week.PlanTemplateId));
        }

        await _db.SaveChangesAsync(ct);

        return Result.Success(new UserPlanDto(
            plan.Id,
            plan.Name,
            trainingType.Code,
            difficultyLevel.Code,
            plan.StartDate,
            plan.EndDate,
            plan.IsActive,
            0));
    }
}

internal static class PlanPhaseHelper
{
    private static readonly System.Text.RegularExpressions.Regex WeekRangeRegex =
        new(@"Semanas?\s+(?<start>\d+)\s*-\s*(?<end>\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

    public static List<GeneratedWeek> BuildWeeks(
        IEnumerable<Phase> phases,
        DateTime startDate,
        int? planTemplateId)
    {
        var parsedPhases = phases
            .Select(phase => new ParsedPhase(
                phase.Id,
                phase.SortOrder,
                TryParseWeekRange(phase.Description)))
            .ToList();

        var totalWeeks = parsedPhases
            .Where(phase => phase.WeekRange is not null)
            .Select(phase => phase.WeekRange!.Value.End)
            .DefaultIfEmpty(4)
            .Max();

        var generatedWeeks = new List<GeneratedWeek>(capacity: totalWeeks);
        for (var weekNumber = 1; weekNumber <= totalWeeks; weekNumber++)
        {
            var parsedPhase = ResolvePhase(parsedPhases, weekNumber);
            generatedWeeks.Add(new GeneratedWeek(
                weekNumber,
                parsedPhase.PhaseId,
                startDate.AddDays((weekNumber - 1) * 7),
                weekNumber % 4 == 0,
                planTemplateId));
        }

        return generatedWeeks;
    }

    private static ParsedPhase ResolvePhase(List<ParsedPhase> phases, int weekNumber)
    {
        var rangedPhase = phases.FirstOrDefault(phase =>
            phase.WeekRange is not null &&
            weekNumber >= phase.WeekRange.Value.Start &&
            weekNumber <= phase.WeekRange.Value.End);

        return rangedPhase
               ?? phases.OrderBy(phase => phase.SortOrder).Last();
    }

    public static string ExtractColorHex(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "#5C5C5C";
        }

        var match = System.Text.RegularExpressions.Regex.Match(
            description,
            @"#[0-9A-Fa-f]{6}",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        return match.Success ? match.Value : "#5C5C5C";
    }

    private static (int Start, int End)? TryParseWeekRange(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var match = WeekRangeRegex.Match(description);
        if (!match.Success)
        {
            return null;
        }

        return (int.Parse(match.Groups["start"].Value), int.Parse(match.Groups["end"].Value));
    }

    private sealed record ParsedPhase(int PhaseId, int SortOrder, (int Start, int End)? WeekRange);

    internal sealed record GeneratedWeek(
        int WeekNumber,
        int PhaseId,
        DateTime StartDate,
        bool IsDeload,
        int? PlanTemplateId);
}
