using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.CustomSessions.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Entities;
using ClimbTrack.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.CustomSessions.Commands;

public record UpdateCustomSessionCommand(
    long Id,
    string Name,
    string ColorHex,
    int LoadLevel,
    string? Description,
    List<CustomSessionBlockInputDto> Blocks) : IRequest<Result<CustomSessionDto>>;

public class UpdateCustomSessionCommandValidator : AbstractValidator<UpdateCustomSessionCommand>
{
    public UpdateCustomSessionCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.ColorHex).Matches("^#[0-9A-Fa-f]{6}$");
        RuleFor(command => command.LoadLevel).InclusiveBetween(0, 4);
        RuleFor(command => command.Description).MaximumLength(500);
        RuleFor(command => command.Blocks).NotEmpty();
        RuleForEach(command => command.Blocks).SetValidator(new CustomSessionBlockInputValidator());
    }
}

public class UpdateCustomSessionCommandHandler : IRequestHandler<UpdateCustomSessionCommand, Result<CustomSessionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateCustomSessionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CustomSessionDto>> Handle(UpdateCustomSessionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<CustomSessionDto>("Authentication is required.");
        }

        var session = await _db.UserCustomSessions
            .Include(entity => entity.Blocks)
            .FirstOrDefaultAsync(
                entity => entity.Id == request.Id &&
                          entity.UserId == _currentUser.UserId.Value &&
                          entity.DeletedAt == null,
                cancellationToken);

        if (session is null)
        {
            return Result.Failure<CustomSessionDto>("Custom session not found.");
        }

        session.Update(
            request.Name.Trim(),
            request.ColorHex,
            request.LoadLevel,
            request.Description?.Trim());

        _db.UserCustomSessionBlocks.RemoveRange(session.Blocks);
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

        return Result.Success(new CustomSessionDto(
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
                .ToList()));
    }
}
