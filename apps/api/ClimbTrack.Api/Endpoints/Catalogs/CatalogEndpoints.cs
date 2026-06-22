using ClimbTrack.Application.Features.Catalogs.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Catalogs;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogs(this RouteGroupBuilder group)
    {
        group.MapGet("/session-types", GetSessionTypes)
            .WithName("Catalogs_GetSessionTypes")
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetSessionTypes(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetSessionTypesQuery(), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CatalogEndpoints")
                .LogError(ex, "Failed to load session types.");
            return Results.Problem("An unexpected server error occurred while loading session types.");
        }
    }
}
