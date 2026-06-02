using FluentValidation;

namespace ClimbTrack.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.Level)
            .NotEmpty()
            .Must(level => level is "novato" or "intermedio" or "avanzado")
            .WithMessage("Level must be novato, intermedio, or avanzado.");
    }
}

