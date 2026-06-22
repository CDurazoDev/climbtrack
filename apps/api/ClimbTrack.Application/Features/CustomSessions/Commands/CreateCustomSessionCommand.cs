using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.CustomSessions.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Entities;
using ClimbTrack.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ClimbTrack.Application.Features.CustomSessions.Commands;

public record CreateCustomSessionCommand(
    string Name,
    string ColorHex,
    int LoadLevel,
    string? Description,
    List<CustomSessionBlockInputDto> Blocks) : IRequest<Result<CustomSessionDto>>;

public class CreateCustomSessionCommandValidator : AbstractValidator<CreateCustomSessionCommand>
{
    public CreateCustomSessionCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.ColorHex).Matches("^#[0-9A-Fa-f]{6}$");
        RuleFor(command => command.LoadLevel).InclusiveBetween(0, 4);
        RuleFor(command => command.Description).MaximumLength(500);
        RuleFor(command => command.Blocks).NotEmpty();
        RuleForEach(command => command.Blocks).SetValidator(new CustomSessionBlockInputValidator());
    }
}

public class CreateCustomSessionCommandHandler : IRequestHandler<CreateCustomSessionCommand, Result<CustomSessionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateCustomSessionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CustomSessionDto>> Handle(CreateCustomSessionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<CustomSessionDto>("Authentication is required.");
        }

        var session = new UserCustomSession(
            _currentUser.UserId.Value,
            request.Name.Trim(),
            request.ColorHex,
            request.LoadLevel,
            request.Description?.Trim());

        _db.UserCustomSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);

        session.ReplaceBlocks(
            request.Blocks
                .OrderBy(block => block.SortOrder)
                .Select((block, index) => new UserCustomSessionBlock(
                    session.Id,
                    block.Name.Trim(),
                    index,
                    block.Items))
                .ToList());

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(MapDto(session));
    }

    private static CustomSessionDto MapDto(UserCustomSession session)
    {
        return new CustomSessionDto(
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
                .ToList());
    }
}

internal sealed class CustomSessionBlockInputValidator : AbstractValidator<CustomSessionBlockInputDto>
{
    public CustomSessionBlockInputValidator()
    {
        RuleFor(block => block.Name).NotEmpty().MaximumLength(50);
        RuleFor(block => block.Items).NotEmpty();
        RuleForEach(block => block.Items).NotEmpty().MaximumLength(120);
    }
}
