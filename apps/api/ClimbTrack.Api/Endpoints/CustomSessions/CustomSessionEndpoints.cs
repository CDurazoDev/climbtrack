using ClimbTrack.Application.Features.CustomSessions.Commands;
using ClimbTrack.Application.Features.CustomSessions.Dtos;
using ClimbTrack.Application.Features.CustomSessions.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.CustomSessions;

public static class CustomSessionEndpoints
{
    public static RouteGroupBuilder MapCustomSessions(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetCustomSessions)
            .WithName("CustomSessions_List")
            .RequireAuthorization();

        group.MapPost("/", CreateCustomSession)
            .WithName("CustomSessions_Create")
            .RequireAuthorization()
            .Produces<CustomSessionDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:long}", UpdateCustomSession)
            .WithName("CustomSessions_Update")
            .RequireAuthorization()
            .Produces<CustomSessionDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:long}", DeleteCustomSession)
            .WithName("CustomSessions_Delete")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetCustomSessions(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetCustomSessionsQuery(), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CustomSessionEndpoints")
                .LogError(ex, "Failed to load custom sessions.");
            return Results.Problem("An unexpected server error occurred while loading custom sessions.");
        }
    }

    private static async Task<IResult> CreateCustomSession(
        CreateCustomSessionCommand command,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/custom-sessions/{result.Value.Id}", result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CustomSessionEndpoints")
                .LogError(ex, "Failed to create custom session.");
            return Results.Problem("An unexpected server error occurred while creating the custom session.");
        }
    }

    private static async Task<IResult> UpdateCustomSession(
        long id,
        CreateCustomSessionRequest request,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new UpdateCustomSessionCommand(
                    id,
                    request.Name,
                    request.ColorHex,
                    request.LoadLevel,
                    request.Description,
                    request.Blocks),
                cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CustomSessionEndpoints")
                .LogError(ex, "Failed to update custom session {CustomSessionId}.", id);
            return Results.Problem("An unexpected server error occurred while updating the custom session.");
        }
    }

    private static async Task<IResult> DeleteCustomSession(
        long id,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new DeleteCustomSessionCommand(id), cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("CustomSessionEndpoints")
                .LogError(ex, "Failed to delete custom session {CustomSessionId}.", id);
            return Results.Problem("An unexpected server error occurred while deleting the custom session.");
        }
    }
}
