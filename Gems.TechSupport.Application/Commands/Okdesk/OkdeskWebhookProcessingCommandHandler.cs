using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Abstractions.Responses;
using Gems.TechSupport.Application.Commands.Issues.AddIssues;
using Gems.TechSupport.Application.Responses.Webhooks;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gems.TechSupport.Application.Commands.Okdesk;

internal sealed class OkdeskWebhookProcessingCommandHandler(
    IOkdeskNotificationTemplatesProvider provider,
    IApplicationDbContext dbContext,
    ISender sender,
    IResponseToDomainMapper mapper,
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
                case StatusUpdatedWebhookEvent statusEvent:
                    await HandleStatusUpdate(issue, statusEvent, cancellationToken);
                    break;

                case ProblemUpdatedWebhookEvent problemEvent:
                    await HandleProblemUpdate(issue, problemEvent, cancellationToken);
                    break;
                case TypeUpdatedWebhookEvent typeEvent:
                    await HandleTypeUpdate(issue, typeEvent, cancellationToken);
                    break;

                default:
                    logger.LogWarning("Unhandled event type: {EventType}", webhookEvent.GetType().Name);
                    break;
            }
        }
        catch (Exception e)
        {
            logger.LogError(
                    e,
                    "Error Okdesk Webhook process. Issueid:{Issueid}, EventType={EventType}",
                    issue.Id,
                    webhookEvent.GetType().Name);
        }
    }

    private async Task<Issue?> GetIssueFromDb(Issue issue, CancellationToken cancellationToken)
    {
        var issueInDb = await dbContext.Issues
            .Include(x => x.Assignee)
            .Include(x => x.Priority)
            .Include(x => x.Company)
            .Include(x => x.Contact)
            .Include(x => x.Type)
            .FirstOrDefaultAsync(x => x.Id == issue.Id, cancellationToken);
        return issueInDb;
    }

    private async Task HandlePriorityUpdate(
        Issue issue,
        PriorityUpdatedWebhookEvent priorityUpdatedEvent,
        CancellationToken cancellationToken)
    {
        var issueInDb = await GetIssueFromDb(issue, cancellationToken);

        if (issueInDb is null)
        {
            var issueToAddInDb = Issue.CreateNew(
            id: issue.Id,
            title: issue.Title,
            description: issue.Description,
            priority: mapper.ToDomain(priorityUpdatedEvent.OldPriority),
            status: issue.Status,
            type: issue.Type,
            createdAt: issue.CreatedAt,
            updatedAt: issue.UpdatedAt,
            deadlineAt: issue.DeadlineAt,
            completedAt: issue.CompletedAt,
            company: issue.Company,
            contact: issue.Contact,
            assignee: issue.Assignee
          );

            await CreateNewIssue(issueToAddInDb, cancellationToken);

            issueInDb = issueToAddInDb;
        }

        var authorType = priorityUpdatedEvent.Author.Type;
        issueInDb.UpdatePriority(issue, authorType);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleStatusUpdate(
     Issue issue,
     StatusUpdatedWebhookEvent statusUpdatedEvent,
     CancellationToken cancellationToken)
    {
        var issueInDb = await GetIssueFromDb(issue, cancellationToken);

        if (issueInDb is null)
        {
            var issueToAddInDb = Issue.CreateNew(
            id: issue.Id,
            title: issue.Title,
            description: issue.Description,
            priority: issue.Priority,
            status: mapper.ToDomain(statusUpdatedEvent.OldStatus),
            type: issue.Type,
            createdAt: issue.CreatedAt,
            updatedAt: issue.UpdatedAt,
            deadlineAt: issue.DeadlineAt,
            completedAt: issue.CompletedAt,
            company: issue.Company,
            contact: issue.Contact,
            assignee: issue.Assignee
          );

            await CreateNewIssue(issueToAddInDb, cancellationToken);

            issueInDb = issueToAddInDb;
        }


        issueInDb.UpdateStatus(issue);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleProblemUpdate(
        Issue issue,
        ProblemUpdatedWebhookEvent problemEvent,
        CancellationToken cancellationToken)
    {
        var issueInDb = await GetIssueFromDb(issue, cancellationToken);

        if (issueInDb is null)
        {
            var issueToAddInDb = Issue.CreateNew(
            id: issue.Id,
            title: issue.Title,
            description: issue.Description,
            priority: issue.Priority,
            status: issue.Status,
            type: issue.Type,
            createdAt: issue.CreatedAt,
            updatedAt: issue.UpdatedAt,
            deadlineAt: issue.DeadlineAt,
            completedAt: issue.CompletedAt,
            company: issue.Company,
            contact: issue.Contact,
            assignee: issue.Assignee
          );
            await CreateNewIssue(issueToAddInDb, cancellationToken);

            issueInDb = issueToAddInDb;
        }

        var problemParameters = problemEvent.Parameters.First(x => x.Code!.Equals(Constants.AutoCloseProblemNames.CodeProblem));

        if (problemParameters != null && provider.GetProblemTemplate(problemParameters.NewProblem) != null)
        {
            issueInDb.UpdateProblem(problemParameters.NewProblem);
        }
        else
        {
            return;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    private async Task HandleTypeUpdate(
    Issue issue,
    TypeUpdatedWebhookEvent typeUpdatedEvent,
    CancellationToken cancellationToken)
    {
        var issueInDb = await GetIssueFromDb(issue, cancellationToken);

        if (issueInDb is null)
        {
            var issueToAddInDb = Issue.CreateNew(
            id: issue.Id,
            title: issue.Title,
            description: issue.Description,
            priority: issue.Priority,
            status: issue.Status,
            type: mapper.ToDomain(typeUpdatedEvent.OldType),
            createdAt: issue.CreatedAt,
            updatedAt: issue.UpdatedAt,
            deadlineAt: issue.DeadlineAt,
            completedAt: issue.CompletedAt,
            company: issue.Company,
            contact: issue.Contact,
            assignee: issue.Assignee
          );

            await CreateNewIssue(issueToAddInDb, cancellationToken);

            issueInDb = issueToAddInDb;
        }

        var newType = mapper.ToDomain(typeUpdatedEvent.NewType);

        issueInDb.UpdateType(newType);

        await dbContext.SaveChangesAsync(cancellationToken);

    }
    private async Task CreateNewIssue(Issue issueToAddInDb, CancellationToken cancellationToken)
    {
        var issuesToAddInDb = new List<Issue> { issueToAddInDb };
        var addIssuesCommand = new AddIssuesCommand(issuesToAddInDb);
        await sender.Send(addIssuesCommand, cancellationToken);
    }
}
