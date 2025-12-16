using MediatR;
using Microsoft.Extensions.Logging;

namespace Gems.TechSupport.Application.Exceptions.Handler;

internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>(
    ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(@"Unhandled exception occured while processing {requestName} request
            {exceptionType}: {exceptionMessage}", typeof(TRequest).Name, exception.GetType(), exception.Message);

            throw;
        }
    }
}
