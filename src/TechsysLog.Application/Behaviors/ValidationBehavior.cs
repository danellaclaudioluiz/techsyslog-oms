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
            return CreateFailureResult(errorMessage);
        }

        return await next();
    }

    private static TResponse CreateFailureResult(string error)
    {
        var responseType = typeof(TResponse);

        // Handle Result<TValue> generic type
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            
            // Use the generic Failure<T> method directly
            var failureMethod = typeof(Result)
                .GetMethods()
                .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
                .MakeGenericMethod(valueType);
            
            return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
        }

        // Handle non-generic Result
        var nonGenericFailure = typeof(Result)
            .GetMethods()
            .First(m => m.Name == nameof(Result.Failure) && !m.IsGenericMethod);
        
        return (TResponse)nonGenericFailure.Invoke(null, new object[] { error })!;
    }
}