using ClimbTrack.Application.Features.SessionLogs.Commands;
using FluentValidation;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.CompleteSessionLog;

public sealed class CompleteSessionLogCommandValidator : AbstractValidator<CompleteSessionLogCommand>
{
    public CompleteSessionLogCommandValidator()
    {
        RuleFor(command => command.SessionLogId)
            .GreaterThan(0);

        RuleFor(command => command.Rpe)
            .InclusiveBetween(1, 10);

        RuleFor(command => command.DurationMin)
            .GreaterThan(0)
            .LessThanOrEqualTo(1440);

        RuleForEach(command => command.Metrics)
            .SetValidator(new SessionLogMetricValidator());
    }
}
