using ClimbTrack.Application.Features.SessionLogs.Dtos;
using FluentValidation;

namespace ClimbTrack.Application.Features.SessionLogs.Commands;

public sealed class SessionLogMetricValidator : AbstractValidator<MetricInput>
{
    public SessionLogMetricValidator()
    {
        RuleFor(metric => metric.Key)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(metric => metric.Value)
            .NotEmpty();

        RuleFor(metric => metric.Unit)
            .MaximumLength(20);
    }
}
