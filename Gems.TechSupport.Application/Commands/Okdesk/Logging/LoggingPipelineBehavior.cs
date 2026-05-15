using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Gems.TechSupport.Application.Commands.Okdesk.Logging;
internal sealed class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger,
    IFeatureManager featureManager) : IPipelineBehavior<TRequest, TResponse> where TRequest : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if(request is OkdeskWebhookProcessingCommand webhookCommand
            && await featureManager.IsEnabledAsync(Constants.LoggingFeatures.WebhookRequestLogging)) {
            logger
                .LogInformation("WebhookEvent: {@WebhookEvent} on Issue: {@Issue}", webhookCommand.WebhookEvent, webhookCommand.Issue);
        }
        return await next(cancellationToken);
    }
}
