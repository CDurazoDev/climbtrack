using ClimbTrack.Application.Features.SessionLogs.Commands;
using FluentValidation;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.UpdateSessionLog;

public sealed class UpdateSessionLogCommandValidator : AbstractValidator<UpdateSessionLogCommand>
{
    public UpdateSessionLogCommandValidator()
    {
        RuleFor(command => command.SessionLogId)
            .GreaterThan(0);

        RuleFor(command => command.Rpe)
            .InclusiveBetween(1, 10)
            .When(command => command.Rpe.HasValue);

        RuleFor(command => command.DurationMin)
            .GreaterThan(0)
            .LessThanOrEqualTo(1440)
            .When(command => command.DurationMin.HasValue);

        RuleForEach(command => command.Metrics)
            .SetValidator(new SessionLogMetricValidator());
    }
}
