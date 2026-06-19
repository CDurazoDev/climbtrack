using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result>;
