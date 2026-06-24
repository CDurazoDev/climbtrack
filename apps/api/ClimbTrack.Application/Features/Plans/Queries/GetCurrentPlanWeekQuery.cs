using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Plans.Queries;

public record GetCurrentPlanWeekQuery : IRequest<Result<PlanWeekDto>>;

public class GetCurrentPlanWeekQueryHandler : IRequestHandler<GetCurrentPlanWeekQuery, Result<PlanWeekDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentPlanWeekQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PlanWeekDto>> Handle(GetCurrentPlanWeekQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<PlanWeekDto>("Authentication is required.");
        }

        var today = DateTime.UtcNow.Date;
        var activeWeek = await _db.UserPlanWeeks
            .AsNoTracking()
            .Include(week => week.Phase)
            .Include(week => week.Plan)
                .ThenInclude(plan => plan.TrainingType)
            .Include(week => week.Plan)
                .ThenInclude(plan => plan.DifficultyLevel)
            .Where(week =>
                week.Plan.UserId == _currentUser.UserId.Value &&
                week.Plan.IsActive &&
                week.StartDate <= today &&
                week.StartDate.AddDays(7) > today)
            .OrderBy(week => week.WeekNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeWeek is null)
        {
            return Result.Failure<PlanWeekDto>("Current plan week not found.");
        }

        var weekDtos = await PlanQueryMapper.MapWeeksAsync(
            _db,
            _currentUser.UserId.Value,
            [activeWeek],
            cancellationToken);

        return Result.Success(weekDtos[0]);
    }
}
