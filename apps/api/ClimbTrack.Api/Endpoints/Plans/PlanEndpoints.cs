using ClimbTrack.Application.Features.Plans.Commands.CreateUserPlan;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Application.Features.Plans.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Plans;

public static class PlanEndpoints
{
    public static RouteGroupBuilder MapPlans(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetUserPlans).WithName("GetUserPlans").RequireAuthorization();
        group.MapGet("/active", GetActivePlan).WithName("GetActivePlan").RequireAuthorization();
        group.MapPost("/", CreatePlan)
            .WithName("CreatePlan")
            .RequireAuthorization()
            .Produces<UserPlanDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        group.MapGet("/{id:long}/weeks", GetPlanWeeks).WithName("GetPlanWeeks").RequireAuthorization();
        return group;
    }

    private static async Task<IResult> GetUserPlans(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetUserPlansQuery(), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to get user plans.");
            return Results.Problem("An unexpected server error occurred while loading plans.");
        }
    }

    private static async Task<IResult> GetActivePlan(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetActivePlanQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to get active plan.");
            return Results.Problem("An unexpected server error occurred while loading the active plan.");
        }
    }

    private static async Task<IResult> CreatePlan(
        CreateUserPlanCommand command,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/plans/{result.Value.Id}", result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to create plan.");
            return Results.Problem("An unexpected server error occurred while creating the plan.");
        }
    }

    private static async Task<IResult> GetPlanWeeks(
        long id,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetPlanWeeksQuery(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to get plan weeks for {PlanId}.", id);
            return Results.Problem("An unexpected server error occurred while loading the plan weeks.");
        }
    }
}
