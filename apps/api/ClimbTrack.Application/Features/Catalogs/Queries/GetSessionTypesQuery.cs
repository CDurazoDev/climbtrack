using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Catalogs.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Catalogs.Queries;

public record GetSessionTypesQuery : IRequest<List<SessionTypeDto>>;

public class GetSessionTypesQueryHandler : IRequestHandler<GetSessionTypesQuery, List<SessionTypeDto>>
{
    private readonly IApplicationDbContext _db;

    public GetSessionTypesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<SessionTypeDto>> Handle(GetSessionTypesQuery request, CancellationToken cancellationToken)
    {
        var sessionTypes = await _db.SessionTypes
            .AsNoTracking()
            .Include(sessionType => sessionType.EnergySystem)
            .Include(sessionType => sessionType.Blocks)
            .ThenInclude(block => block.Items)
            .OrderBy(sessionType => sessionType.Id)
            .ToListAsync(cancellationToken);

        return sessionTypes
            .Select(sessionType => new SessionTypeDto(
                sessionType.Id,
                sessionType.Code,
                sessionType.Name,
                sessionType.ColorHex,
                sessionType.LoadLevel,
                sessionType.Description,
                sessionType.EnergySystem.Code,
                sessionType.EnergySystem.Name,
                sessionType.Blocks
                    .OrderBy(block => block.SortOrder)
                    .Select(block => new SessionBlockDto(
                        block.Id,
                        block.Name,
                        block.SortOrder,
                        block.Items
                            .OrderBy(item => item.SortOrder)
                            .Select(item => item.Description)
                            .ToList()))
                    .ToList()))
            .ToList();
    }
}
