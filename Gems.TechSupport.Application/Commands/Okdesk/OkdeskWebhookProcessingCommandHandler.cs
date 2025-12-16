using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Responses.Webhooks;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gems.TechSupport.Application.Commands.Okdesk;

internal sealed class OkdeskWebhookProcessingCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<OkdeskWebhookProcessingCommandHandler> logger)
    : ICommandHandler<OkdeskWebhookProcessingCommand>
{
    public async Task Handle(OkdeskWebhookProcessingCommand request, CancellationToken cancellationToken)
    {
        var issue = request.Issue;
        var webhookEvent = request.WebhookEvent;

        try
        {
            switch (webhookEvent) 
            {
                case PriorityUpdatedWebhookEvent priorityEvent:
                    await HandlePriorityUpdate(issue, priorityEvent, cancellationToken);
                    break;

                default:
                    logger.LogWarning("Unhandled event type: {EventType}", webhookEvent.GetType().Name);
                    break;
            }
        }
        catch (Exception e)
        {
            logger.LogError("{ExceptionMessage}", e.Message);
        }
    }

    private async Task HandlePriorityUpdate(
        Issue issue,
        PriorityUpdatedWebhookEvent priorityUpdatedEvent,
        CancellationToken cancellationToken)
    {
        var issueInDb = await dbContext.Issues
            .Include(x => x.Assignee)
            .Include(x => x.Contact)
            .FirstOrDefaultAsync(x => x.Id == issue.Id, cancellationToken);

        if (issueInDb is null)
        {
            return;
        }

        var authorType = priorityUpdatedEvent.Author.Type;
        issueInDb.UpdatePriority(issue, authorType);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
