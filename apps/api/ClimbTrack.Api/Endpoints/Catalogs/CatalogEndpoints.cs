using ClimbTrack.Application.Features.Catalogs.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Catalogs;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogs(this RouteGroupBuilder group)
    {
        group.MapGet("/session-types", GetSessionTypes).WithName("GetSessionTypes");
        group.MapGet("/session-types/{id:int}", GetSessionTypeById).WithName("GetSessionTypeById");
        group.MapGet("/phases", GetPhases).WithName("GetPhases");
        group.MapGet("/difficulty-levels", GetDifficultyLevels).WithName("GetDifficultyLevels");
        return group;
    }

    private static async Task<IResult> GetSessionTypes(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetSessionTypesQuery(), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CatalogEndpoints").LogError(ex, "Failed to get session types.");
            return Results.Problem("An unexpected server error occurred while loading session types.");
        }
    }

    private static async Task<IResult> GetSessionTypeById(
        int id,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetSessionTypeByIdQuery(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CatalogEndpoints").LogError(ex, "Failed to get session type {SessionTypeId}.", id);
            return Results.Problem("An unexpected server error occurred while loading the session type.");
        }
    }

    private static async Task<IResult> GetPhases(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetPhasesQuery(), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CatalogEndpoints").LogError(ex, "Failed to get phases.");
            return Results.Problem("An unexpected server error occurred while loading phases.");
        }
    }

    private static async Task<IResult> GetDifficultyLevels(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetDifficultyLevelsQuery(), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CatalogEndpoints").LogError(ex, "Failed to get difficulty levels.");
            return Results.Problem("An unexpected server error occurred while loading difficulty levels.");
        }
    }
}
