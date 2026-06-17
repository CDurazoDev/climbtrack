using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Application.Features.Auth.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClimbTrack.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthTokensDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        ITokenService tokenService,
        ILogger<RegisterCommandHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<AuthTokensDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(user => user.Email == request.Email, ct))
        {
            _logger.LogInformation("Register rejected because email already exists.");
            return Result.Failure<AuthTokensDto>("Email already registered.");
        }

        var level = await _db.DifficultyLevels
            .FirstOrDefaultAsync(difficultyLevel => difficultyLevel.Code == request.Level, ct);

        if (level is null)
        {
            _logger.LogWarning(
                "Register rejected because difficulty level catalog is missing or level '{LevelCode}' does not exist.",
                request.Level);
            return Result.Failure<AuthTokensDto>(
                "Unable to register because the difficulty levels catalog is not initialized correctly.");
        }

        var passwordHash = _hasher.Hash(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, level.Id);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return Result.Success(await _tokenService.GenerateTokensAsync(user, ct));
    }
}

