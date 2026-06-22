using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Users.Commands.UpdateLocale;

public record UpdateLocaleCommand(string Locale) : IRequest<Result>;

public class UpdateLocaleCommandValidator : AbstractValidator<UpdateLocaleCommand>
{
    public UpdateLocaleCommandValidator()
    {
        RuleFor(command => command.Locale)
            .NotEmpty()
            .Must(locale => locale is "es" or "en");
    }
}

public class UpdateLocaleCommandHandler : IRequestHandler<UpdateLocaleCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateLocaleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateLocaleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure("Authentication is required.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(
            entity => entity.Id == _currentUser.UserId.Value,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        user.SetPreferredLocale(request.Locale);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
