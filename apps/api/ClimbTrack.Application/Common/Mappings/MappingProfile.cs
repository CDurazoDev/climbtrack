using AutoMapper;
using ClimbTrack.Application.Features.Catalogs.Dtos;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Entities;

namespace ClimbTrack.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SessionType, SessionTypeDto>()
            .ForMember(d => d.EnergySystemCode, o => o.MapFrom(s => s.EnergySystem.Code))
            .ForMember(d => d.EnergySystemName, o => o.MapFrom(s => s.EnergySystem.Name))
            .ForMember(d => d.Blocks, o => o.MapFrom(s => s.Blocks.OrderBy(b => b.SortOrder)));

        CreateMap<SessionBlock, SessionBlockDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s =>
                s.Items.OrderBy(i => i.SortOrder).Select(i => i.Description).ToList()));

        CreateMap<UserPlan, UserPlanDto>()
            .ForMember(d => d.TrainingTypeCode, o => o.MapFrom(s => s.TrainingType.Code))
            .ForMember(d => d.DifficultyLevelCode, o => o.MapFrom(s => s.DifficultyLevel.Code))
            .ForMember(
                d => d.ProgressPct,
                o => o.MapFrom(s => s.Weeks.Count == 0 ? 0 : s.Weeks.Average(week => week.ProgressPct)));

        CreateMap<UserSessionLog, SessionLogDto>()
            .ForMember(d => d.SessionTypeId, o => o.MapFrom(s => s.SessionType.Code))
            .ForMember(d => d.SessionTypeName, o => o.MapFrom(s => s.SessionType.Name))
            .ForMember(d => d.SessionColorHex, o => o.MapFrom(s => s.SessionType.ColorHex));

        CreateMap<SessionLogMetric, SessionMetricDto>()
            .ForMember(d => d.MetricKey, o => o.MapFrom(s => s.MetricKey))
            .ForMember(d => d.MetricValue, o => o.MapFrom(s => s.MetricValue))
            .ForMember(d => d.MetricUnit, o => o.MapFrom(s => s.MetricUnit));
    }
}
