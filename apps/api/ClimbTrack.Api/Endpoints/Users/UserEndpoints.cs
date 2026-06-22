using ClimbTrack.Application.Features.Users.Commands.UpdateLocale;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Users;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUsers(this RouteGroupBuilder group)
    {
        group.MapPatch("/me/locale", UpdateLocale)
            .WithName("Users_UpdateLocale")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> UpdateLocale(
        UpdateLocaleCommand command,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("UserEndpoints")
                .LogError(ex, "Failed to update locale.");
            return Results.Problem("An unexpected server error occurred while updating the locale.");
        }
    }
}
