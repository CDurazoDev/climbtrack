using ClimbTrack.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClimbTrack.Application.Common.Behaviors;

public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {RequestName}", typeof(TRequest).Name);
            return CreateFailureResponse();
        }
    }

    private static TResponse CreateFailureResponse()
    {
        const string error = "An unexpected server error occurred. Please try again.";

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var method = typeof(Result)
                .GetMethods()
                .Single(method =>
                    method.Name == nameof(Result.Failure) &&
                    method.IsGenericMethodDefinition &&
                    method.GetParameters().Length == 1)
                .MakeGenericMethod(valueType);

            return (TResponse)method.Invoke(null, [error])!;
        }

        throw new InvalidOperationException("ExceptionHandlingBehavior requires Result response types.");
    }
}
