namespace ClimbTrack.Application.Features.CustomSessions.Dtos;

public record CustomSessionDto(
    long Id,
    string Name,
    string ColorHex,
    int LoadLevel,
    string? Description,
    List<CustomSessionBlockDto> Blocks);

public record CustomSessionBlockDto(
    long Id,
    string Name,
    int SortOrder,
    List<string> Items);

public record CustomSessionBlockInputDto(
    string Name,
    int SortOrder,
    List<string> Items);

public record CreateCustomSessionRequest(
    string Name,
    string ColorHex,
    int LoadLevel,
    string? Description,
    List<CustomSessionBlockInputDto> Blocks);
