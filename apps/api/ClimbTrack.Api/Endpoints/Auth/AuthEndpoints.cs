using ClimbTrack.Application.Features.Auth.Commands.Login;
using ClimbTrack.Application.Features.Auth.Commands.Logout;
using ClimbTrack.Application.Features.Auth.Commands.RefreshToken;
using ClimbTrack.Application.Features.Auth.Commands.Register;
using ClimbTrack.Application.Features.Auth.Dtos;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuth(this RouteGroupBuilder group)
    {
        group.MapPost("/register", Register)
            .WithName("Auth_Register")
            .AllowAnonymous()
            .Produces<AuthTokensDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/login", Login)
            .WithName("Auth_Login")
            .AllowAnonymous()
            .Produces<AuthTokensDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", Refresh)
            .WithName("Auth_Refresh")
            .AllowAnonymous()
            .Produces<AuthTokensDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .WithName("Auth_Logout")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        return group;
    }

    private static async Task<IResult> Register(
        RegisterCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.Created("/me", result.Value)
            : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Login(
        LoginCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error, statusCode: StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Refresh(
        RefreshTokenCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error, statusCode: StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Logout(
        LogoutCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }
}
