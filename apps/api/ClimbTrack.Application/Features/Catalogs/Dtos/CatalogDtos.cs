namespace ClimbTrack.Application.Features.Catalogs.Dtos;

public record SessionTypeDto(
    int Id,
    string Code,
    string Name,
    string ColorHex,
    int LoadLevel,
    string? Description,
    string EnergySystemCode,
    string EnergySystemName,
    List<SessionBlockDto> Blocks);

public record SessionBlockDto(int Id, string Name, int SortOrder, List<string> Items);

public record PhaseDto(int Id, string Code, string Name, int SortOrder);

public record DifficultyLevelDto(int Id, string Code, string Name, int MaxDaysWeek);
