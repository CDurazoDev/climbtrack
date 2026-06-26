using ClimbTrack.Application.Features.SessionLogs.Commands;
using FluentValidation;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.CreateSessionLog;

public sealed class CreateSessionLogCommandValidator : AbstractValidator<CreateSessionLogCommand>
{
    public CreateSessionLogCommandValidator()
    {
        RuleFor(command => command.UserPlanWeekId)
            .GreaterThan(0)
            .When(command => command.UserPlanWeekId.HasValue);

        RuleFor(command => command.SessionTypeId)
            .NotEmpty();

        RuleFor(command => command.DayOfWeek)
            .InclusiveBetween(0, 6);

        RuleFor(command => command.LogDate)
            .NotEqual(default(DateOnly));

        RuleFor(command => command)
            .Must(command =>
                command.DayOfWeek == SessionLogDayOfWeekHelper.ToClimbTrackDayOfWeek(command.LogDate))
            .WithMessage("DayOfWeek must match the provided LogDate using the Monday-to-Sunday convention.");
    }
}
