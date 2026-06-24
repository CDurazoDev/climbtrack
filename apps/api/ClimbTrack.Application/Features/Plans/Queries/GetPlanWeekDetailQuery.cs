using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Plans.Queries;

public record GetPlanWeekDetailQuery(long PlanId, int WeekNumber) : IRequest<Result<PlanWeekDto>>;

public class GetPlanWeekDetailQueryHandler : IRequestHandler<GetPlanWeekDetailQuery, Result<PlanWeekDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetPlanWeekDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PlanWeekDto>> Handle(GetPlanWeekDetailQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<PlanWeekDto>("Authentication is required.");
        }

        var week = await _db.UserPlanWeeks
            .AsNoTracking()
            .Include(planWeek => planWeek.Phase)
            .Include(planWeek => planWeek.Plan)
            .Where(planWeek =>
                planWeek.UserPlanId == request.PlanId &&
                planWeek.WeekNumber == request.WeekNumber &&
                planWeek.Plan.UserId == _currentUser.UserId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (week is null)
        {
            return Result.Failure<PlanWeekDto>("Plan week not found.");
        }

        var weekDtos = await PlanQueryMapper.MapWeeksAsync(
            _db,
            _currentUser.UserId.Value,
            [week],
            cancellationToken);

        return Result.Success(weekDtos[0]);
    }
}
