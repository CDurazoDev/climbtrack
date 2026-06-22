using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.CustomSessions.Dtos;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.CustomSessions.Queries;

public record GetCustomSessionsQuery : IRequest<List<CustomSessionDto>>;

public class GetCustomSessionsQueryHandler : IRequestHandler<GetCustomSessionsQuery, List<CustomSessionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCustomSessionsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<CustomSessionDto>> Handle(GetCustomSessionsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return [];
        }

        var sessions = await _db.UserCustomSessions
            .AsNoTracking()
            .Include(session => session.Blocks)
            .Where(session => session.UserId == _currentUser.UserId.Value && session.DeletedAt == null)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(session => new CustomSessionDto(
                session.Id,
                session.Name,
                session.ColorHex,
                session.LoadLevel,
                session.Description,
                session.Blocks
                    .OrderBy(block => block.SortOrder)
                    .Select(block => new CustomSessionBlockDto(
                        block.Id,
                        block.Name,
                        block.SortOrder,
                        block.GetItems().ToList()))
                    .ToList()))
            .ToList();
    }
}
