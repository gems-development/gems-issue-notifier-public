using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Application.Commands.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Shared.CQRS;
using Microsoft.Extensions.Logging;

namespace Gems.TechSupport.Application.EventHandlers;

internal sealed class IssuePriorityUpdatedEventHandler(
    ITelegramService telegramService,
    IOkdeskService okdeskService,
    IOkdeskNotificationTemplatesProvider notificationProvider,
    ILogger<IssuePriorityUpdatedEventHandler> logger)
    : IDomainEventHandler<IssuePriorityUpdatedEvent>
{
    public async Task Handle(IssuePriorityUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var authorType = notification.UpdateAuthorType;

        switch (authorType)
        {
            case Constants.IssueUpdateAuthorType.Employee:
                await HandleEmployeeUpdate(notification, cancellationToken);
                break;

            case Constants.IssueUpdateAuthorType.Contact:
                await HandleContactUpdate(notification, cancellationToken);
                break;

            default:
                logger.LogWarning("Unhandled issue ({IssueId}) priority update author type: {AuthorType}", notification.IssueId, authorType);
                break;
        }
    }

    private Task HandleEmployeeUpdate(IssuePriorityUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var commentTemplate = GetCommentTemplate(notification.Priority);

        if (string.IsNullOrWhiteSpace(commentTemplate))
        {
            logger.LogWarning("Unhandled issue ({IssueId}) priority type occured: {Priority}", notification.IssueId, notification.Priority);
            return Task.CompletedTask;
        }

        var comment = commentTemplate.Replace("[contact]", notification.ContactFullName);
        var postCommentRequest = new PostIssueCommentRequest(notification.IssueId, comment, notification.AssigneeId);
        return okdeskService.PostCommentAsync(postCommentRequest, cancellationToken);
    }

    private Task HandleContactUpdate(IssuePriorityUpdatedEvent notification, CancellationToken cancellationToken)
    {
        return telegramService.SendIssuePriorityUpdatedNotificationAsync(
            notification.IssueId,
            notification.AssigneeId,
            notification.ContactFullName,
            notification.Priority, cancellationToken);
    }

    private string GetCommentTemplate(IssuePriority priority)
    {
        return priority switch
        {
            IssuePriority.Highest => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.PriorityUpdatedToHighest),
            IssuePriority.High => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.PriorityUpdatedToHigh),
            IssuePriority.Normal => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.PriorityUpdatedToNormal),
            IssuePriority.Low => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.PriorityUpdatedToLow),
            _ => string.Empty
        };
    }
}
