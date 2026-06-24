using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Plans.Queries;

public record GetActivePlanQuery : IRequest<Result<UserPlanDetailDto>>;

public class GetActivePlanQueryHandler : IRequestHandler<GetActivePlanQuery, Result<UserPlanDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetActivePlanQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<UserPlanDetailDto>> Handle(GetActivePlanQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<UserPlanDetailDto>("Authentication is required.");
        }

        var activePlan = await _db.UserPlans
            .AsNoTracking()
            .Include(plan => plan.TrainingType)
            .Include(plan => plan.DifficultyLevel)
            .Include(plan => plan.Weeks)
                .ThenInclude(week => week.Phase)
            .Where(plan => plan.UserId == _currentUser.UserId.Value && plan.IsActive)
            .OrderByDescending(plan => plan.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (activePlan is null)
        {
            return Result.Failure<UserPlanDetailDto>("Active plan not found.");
        }

        var weekDtos = await PlanQueryMapper.MapWeeksAsync(
            _db,
            _currentUser.UserId.Value,
            activePlan.Weeks,
            cancellationToken);

        return Result.Success(new UserPlanDetailDto(
            activePlan.Id,
            activePlan.Name,
            activePlan.TrainingType.Code,
            activePlan.DifficultyLevel.Code,
            activePlan.StartDate,
            activePlan.EndDate,
            activePlan.IsActive,
            weekDtos));
    }
}
