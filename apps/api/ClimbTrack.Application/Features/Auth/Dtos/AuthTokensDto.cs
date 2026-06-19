namespace ClimbTrack.Application.Features.Auth.Dtos;

public record AuthTokensDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserProfileDto User);

public record UserProfileDto(
    long Id,
    string Name,
    string Email,
    string Role,
    string Level,
    string PreferredLocale);
