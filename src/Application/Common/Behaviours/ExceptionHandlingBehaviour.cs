using MediatR;

using VerticalSliceArchitecture.Application.Common.Models;

namespace VerticalSliceArchitecture.Application.Common.Behaviours;

public class ExceptionHandlingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var message = ex.Message;

            if (!typeof(TResponse).IsGenericType || typeof(TResponse).GetGenericTypeDefinition() != typeof(ApiResponse<>))
            {
                throw;
            }

            var responseType = typeof(TResponse).GetGenericArguments()[0];
            var failMethod = typeof(ApiResponse<>)
                .MakeGenericType(responseType)
                .GetMethod(nameof(ApiResponse<object>.Fail), [typeof(string), typeof(int)]);

            if (failMethod is null)
            {
                throw;
            }

            var result = failMethod.Invoke(null, [message, 500])!;

            return (TResponse)result;
        }
    }
}