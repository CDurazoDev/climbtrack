using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.CustomSessions.Commands;

public record DeleteCustomSessionCommand(long Id) : IRequest<Result>;

public class DeleteCustomSessionCommandHandler : IRequestHandler<DeleteCustomSessionCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteCustomSessionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteCustomSessionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure("Authentication is required.");
        }

        var session = await _db.UserCustomSessions.FirstOrDefaultAsync(
            entity => entity.Id == request.Id &&
                      entity.UserId == _currentUser.UserId.Value &&
                      entity.DeletedAt == null,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure("Custom session not found.");
        }

        session.SoftDelete();
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
