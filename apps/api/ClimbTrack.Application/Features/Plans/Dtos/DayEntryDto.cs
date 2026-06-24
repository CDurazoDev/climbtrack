namespace ClimbTrack.Application.Features.Plans.Dtos;

public record DayEntryDto(
    string Label,
    string State,
    string? SessionTypeId,
    string? SessionTypeName,
    string? SessionColorHex);
