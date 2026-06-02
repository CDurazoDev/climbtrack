using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Application.Features.Auth.Dtos;
using ClimbTrack.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthTokensDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        ITokenService tokenService)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokensDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(user => user.DifficultyLevel)
            .FirstOrDefaultAsync(user => user.Email == request.Email && user.IsActive, ct);

        if (user is null || !_hasher.Verify(user.PasswordHash, request.Password))
        {
            return Result.Failure<AuthTokensDto>("Invalid credentials.");
        }

        return Result.Success(await _tokenService.GenerateTokensAsync(user, ct));
    }
}

