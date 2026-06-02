using ClimbTrack.Application.Features.Auth.Dtos;
using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Name,
    string Email,
    string Password,
    string Level) : IRequest<Result<AuthTokensDto>>;

