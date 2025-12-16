using Gems.TechSupport.Application.Commands.Okdesk;
using Gems.TechSupport.Application.Responses;
using Gems.TechSupport.Application.Responses.Webhooks;
using Gems.TechSupport.EndpointsSettings;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Gems.TechSupport.Features.Webhooks;

public sealed class OkdeskWebhooks : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks", 
            async ([FromBody] WebhookPayload webhookPayload, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        {
            var issue = webhookPayload.IssueData.ToDomainExisting();

            var processWebhookCommand = new OkdeskWebhookProcessingCommand(issue, webhookPayload.WebhookEvent);
            await sender.Send(processWebhookCommand, cancellationToken);

            return Results.Accepted();
        });
    }
}