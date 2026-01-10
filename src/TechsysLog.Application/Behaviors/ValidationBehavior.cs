using FluentValidation;
using MediatR;
using TechsysLog.Domain.Common;

namespace TechsysLog.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates requests before handling.
/// Automatically runs all validators registered for the request type.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            
            // Create failure result using reflection to handle both Result and Result<T>
            return CreateFailureResult<TResponse>(errorMessage);
        }

        return await next();
    }

    private static TResponse CreateFailureResult<T>(string error) where T : Result
    {
        // Handle Result<TValue> generic type
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(T).GetGenericArguments()[0];
            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), new[] { typeof(string) })!
                .MakeGenericMethod(valueType);
            
            return (T)failureMethod.Invoke(null, new object[] { error })!;
        }

        // Handle non-generic Result
        return (T)(object)Result.Failure(error);
    }
}