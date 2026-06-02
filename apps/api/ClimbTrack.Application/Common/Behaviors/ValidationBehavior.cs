using ClimbTrack.Domain.Common;
using FluentValidation;
using MediatR;

namespace ClimbTrack.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, ct)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .Select(error => error.ErrorMessage)
            .Distinct()
            .ToArray();

        if (errors.Length == 0)
        {
            return await next();
        }

        return CreateFailureResponse(string.Join("; ", errors));
    }

    private static TResponse CreateFailureResponse(string error)
    {
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

        throw new InvalidOperationException("ValidationBehavior requires Result response types.");
    }
}
