using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Catalogs.Dtos;
using ClimbTrack.Domain.Common;
using Dapper;
using MediatR;

namespace ClimbTrack.Application.Features.Catalogs.Queries;

public record GetSessionTypesQuery : IRequest<List<SessionTypeDto>>;

public record GetSessionTypeByIdQuery(int Id) : IRequest<Result<SessionTypeDto>>;

public record GetPhasesQuery : IRequest<List<PhaseDto>>;

public record GetDifficultyLevelsQuery : IRequest<List<DifficultyLevelDto>>;

public class GetSessionTypesHandler : IRequestHandler<GetSessionTypesQuery, List<SessionTypeDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetSessionTypesHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<SessionTypeDto>> Handle(GetSessionTypesQuery request, CancellationToken ct)
    {
        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var sessionTypes = await CatalogQueryHelper.GetSessionTypesAsync(connection, null);
        return sessionTypes;
    }
}

public class GetSessionTypeByIdHandler : IRequestHandler<GetSessionTypeByIdQuery, Result<SessionTypeDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetSessionTypeByIdHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<SessionTypeDto>> Handle(GetSessionTypeByIdQuery request, CancellationToken ct)
    {
        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var sessionTypes = await CatalogQueryHelper.GetSessionTypesAsync(connection, request.Id);
        var sessionType = sessionTypes.SingleOrDefault();
        return sessionType is null
            ? Result.Failure<SessionTypeDto>("Session type not found.")
            : Result.Success(sessionType);
    }
}

public class GetPhasesHandler : IRequestHandler<GetPhasesQuery, List<PhaseDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetPhasesHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<PhaseDto>> Handle(GetPhasesQuery request, CancellationToken ct)
    {
        const string sql = """
            select id, code, name, sort_order as SortOrder
            from phases
            order by sort_order, id;
            """;

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<CatalogQueryHelper.PhaseRow>(sql);
        return rows
            .Select(row => new PhaseDto(row.Id, row.Code, row.Name, row.SortOrder))
            .ToList();
    }
}

public class GetDifficultyLevelsHandler : IRequestHandler<GetDifficultyLevelsQuery, List<DifficultyLevelDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetDifficultyLevelsHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<DifficultyLevelDto>> Handle(GetDifficultyLevelsQuery request, CancellationToken ct)
    {
        const string sql = """
            select id, code, name, max_days_week as MaxDaysWeek
            from difficulty_levels
            order by id;
            """;

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<CatalogQueryHelper.DifficultyLevelRow>(sql);
        return rows
            .Select(row => new DifficultyLevelDto(row.Id, row.Code, row.Name, row.MaxDaysWeek))
            .ToList();
    }
}

internal static class CatalogQueryHelper
{
    public static async Task<List<SessionTypeDto>> GetSessionTypesAsync(
        System.Data.Common.DbConnection connection,
        int? sessionTypeId)
    {
        const string sessionTypesSql = """
            select st.id,
                   st.code,
                   st.name,
                   st.color_hex as ColorHex,
                   st.load_level as LoadLevel,
                   st.description,
                   es.code as EnergySystemCode,
                   es.name as EnergySystemName
            from session_types st
            inner join energy_systems es on es.id = st.energy_system_id
            where (@SessionTypeId is null or st.id = @SessionTypeId)
            order by st.id;
            """;

        const string blocksSql = """
            select sb.id,
                   sb.session_type_id as SessionTypeId,
                   sb.name,
                   sb.sort_order as SortOrder
            from session_blocks sb
            where (@SessionTypeId is null or sb.session_type_id = @SessionTypeId)
            order by sb.session_type_id, sb.sort_order, sb.id;
            """;

        const string itemsSql = """
            select sbi.session_block_id as SessionBlockId,
                   sbi.description,
                   sbi.sort_order as SortOrder
            from session_block_items sbi
            inner join session_blocks sb on sb.id = sbi.session_block_id
            where (@SessionTypeId is null or sb.session_type_id = @SessionTypeId)
            order by sbi.session_block_id, sbi.sort_order, sbi.id;
            """;

        var types = (await connection.QueryAsync<SessionTypeRow>(sessionTypesSql, new { SessionTypeId = sessionTypeId }))
            .ToDictionary(
                row => row.Id,
                row => new SessionTypeDto(
                    row.Id,
                    row.Code,
                    row.Name,
                    row.ColorHex,
                    row.LoadLevel,
                    row.Description,
                    row.EnergySystemCode,
                    row.EnergySystemName,
                    []));

        if (types.Count == 0)
        {
            return [];
        }

        var blocks = (await connection.QueryAsync<SessionBlockRow>(blocksSql, new { SessionTypeId = sessionTypeId })).ToList();
        var items = (await connection.QueryAsync<SessionBlockItemRow>(itemsSql, new { SessionTypeId = sessionTypeId }))
            .GroupBy(item => item.SessionBlockId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(item => item.SortOrder).Select(item => item.Description).ToList());

        var blocksById = new Dictionary<int, SessionBlockDto>();
        foreach (var block in blocks)
        {
            if (!types.TryGetValue(block.SessionTypeId, out var sessionType))
            {
                continue;
            }

            var sessionBlock = new SessionBlockDto(
                block.Id,
                block.Name,
                block.SortOrder,
                items.TryGetValue(block.Id, out var blockItems) ? blockItems : []);

            sessionType.Blocks.Add(sessionBlock);
            blocksById[block.Id] = sessionBlock;
        }

        return types.Values.OrderBy(type => type.Id).ToList();
    }

    private sealed class SessionTypeRow
    {
        public int Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string ColorHex { get; init; } = string.Empty;
        public int LoadLevel { get; init; }
        public string? Description { get; init; }
        public string EnergySystemCode { get; init; } = string.Empty;
        public string EnergySystemName { get; init; } = string.Empty;
    }

    private sealed class SessionBlockRow
    {
        public int Id { get; init; }
        public int SessionTypeId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int SortOrder { get; init; }
    }

    private sealed class SessionBlockItemRow
    {
        public int SessionBlockId { get; init; }
        public string Description { get; init; } = string.Empty;
        public int SortOrder { get; init; }
    }

    internal sealed class PhaseRow
    {
        public int Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public int SortOrder { get; init; }
    }

    internal sealed class DifficultyLevelRow
    {
        public int Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public int MaxDaysWeek { get; init; }
    }
}
