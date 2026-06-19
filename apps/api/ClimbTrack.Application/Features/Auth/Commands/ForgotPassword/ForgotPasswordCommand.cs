using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
