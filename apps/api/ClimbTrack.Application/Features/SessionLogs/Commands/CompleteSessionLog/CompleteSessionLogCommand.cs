using System.Data;
using AutoMapper;
using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.CompleteSessionLog;

public record MetricInput(string Key, string Value, string? Unit);

public record CompleteSessionLogCommand(
    long SessionLogId,
    int Rpe,
    int DurationMin,
    string? Notes,
    List<MetricInput> Metrics) : IRequest<Result<SessionLogDto>>;

public class CompleteSessionLogCommandValidator : AbstractValidator<CompleteSessionLogCommand>
{
    public CompleteSessionLogCommandValidator()
    {
        RuleFor(command => command.SessionLogId).GreaterThan(0);
        RuleFor(command => command.Rpe).InclusiveBetween(1, 10);
        RuleFor(command => command.DurationMin).GreaterThan(0);
        RuleForEach(command => command.Metrics).SetValidator(new MetricInputValidator());
    }

    private sealed class MetricInputValidator : AbstractValidator<MetricInput>
    {
        public MetricInputValidator()
        {
            RuleFor(metric => metric.Key).NotEmpty().MaximumLength(50);
            RuleFor(metric => metric.Value).NotEmpty();
            RuleFor(metric => metric.Unit).MaximumLength(20);
        }
    }
}

public class CompleteSessionLogCommandHandler : IRequestHandler<CompleteSessionLogCommand, Result<SessionLogDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public CompleteSessionLogCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<Result<SessionLogDto>> Handle(CompleteSessionLogCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<SessionLogDto>("Authentication is required.");
        }

        var sessionLog = await _db.UserSessionLogs
            .Include(log => log.SessionType)
            .FirstOrDefaultAsync(
                log => log.Id == request.SessionLogId && log.UserId == _currentUser.UserId.Value,
                ct);

        if (sessionLog is null)
        {
            return Result.Failure<SessionLogDto>("Session log not found.");
        }

        await _db.UserSessionLogs
            .Where(log => log.Id == request.SessionLogId && log.UserId == _currentUser.UserId.Value)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(log => log.IsDone, true)
                    .SetProperty(log => log.Rpe, request.Rpe)
                    .SetProperty(log => log.DurationMin, request.DurationMin)
                    .SetProperty(log => log.Notes, request.Notes)
                    .SetProperty(log => log.UpdatedAt, DateTime.UtcNow),
                ct);

        if (request.Metrics.Count > 0)
        {
            const string upsertMetricSql = """
                insert into session_log_metrics (session_log_id, metric_key, metric_value, metric_unit)
                values (@SessionLogId, @Key, @Value, @Unit)
                on conflict (session_log_id, metric_key)
                do update set metric_value = excluded.metric_value,
                              metric_unit = excluded.metric_unit;
                """;

            var connection = _db.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(ct);
            }

            var transaction = _db.Database.CurrentTransaction?.GetDbTransaction();
            foreach (var metric in request.Metrics
                         .GroupBy(metric => metric.Key, StringComparer.OrdinalIgnoreCase)
                         .Select(group => group.Last()))
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        upsertMetricSql,
                        new
                        {
                            SessionLogId = request.SessionLogId,
                            metric.Key,
                            metric.Value,
                            Unit = metric.Unit
                        },
                        transaction,
                        cancellationToken: ct));
            }
        }

        var updatedLog = await _db.UserSessionLogs
            .AsNoTracking()
            .Include(log => log.SessionType)
            .FirstAsync(log => log.Id == request.SessionLogId && log.UserId == _currentUser.UserId.Value, ct);

        return Result.Success(_mapper.Map<SessionLogDto>(updatedLog));
    }
}
