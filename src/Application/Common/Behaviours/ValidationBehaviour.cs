using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Mvc;

using VerticalSliceArchitecture.Application.Common.Models;
namespace VerticalSliceArchitecture.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validator is null)
        {
            return await next();
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next();
        }

        if (!typeof(TResponse).IsGenericType || typeof(TResponse).GetGenericTypeDefinition() != typeof(ApiResponse<>))
        {
            throw new InvalidOperationException("ValidationBehaviour only works with ApiResponse<> return types.");
        }

        var responseType = typeof(TResponse).GetGenericArguments()[0];

        var failMethod = typeof(ApiResponse<>)
            .MakeGenericType(responseType)
            .GetMethod(nameof(ApiResponse<object>.Fail), new[] { typeof(string), typeof(int) });

        if (failMethod is null)
        {
            throw new InvalidOperationException("Fail method not found on ApiResponse<>.");
        }

        var message = string.Join(" | ", validationResult.Errors.Select(e => $"{e.ErrorMessage}"));

        var result = failMethod.Invoke(null, [message, 400])!;

        return (TResponse)result;
    }
}