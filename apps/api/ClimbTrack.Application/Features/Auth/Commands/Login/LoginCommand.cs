using ClimbTrack.Application.Features.Auth.Dtos;
using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password)
    : IRequest<Result<AuthTokensDto>>;

