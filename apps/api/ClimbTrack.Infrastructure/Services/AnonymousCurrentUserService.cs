using ClimbTrack.Domain.Interfaces;

namespace ClimbTrack.Infrastructure.Services;

public class AnonymousCurrentUserService : ICurrentUserService
{
    public long? UserId => null;
    public string? Role => null;
}

