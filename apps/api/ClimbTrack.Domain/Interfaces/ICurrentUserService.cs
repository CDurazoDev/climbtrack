namespace ClimbTrack.Domain.Interfaces;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Role { get; }
}
