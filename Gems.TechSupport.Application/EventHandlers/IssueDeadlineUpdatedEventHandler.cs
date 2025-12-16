using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.EventHandlers;

internal sealed class IssueDeadlineUpdatedEventHandler(
    IOkdeskNotificationTemplatesProvider notificationProvider,
    IOkdeskService okdeskService)
    : IDomainEventHandler<IssueDeadlineUpdatedEvent>
{
    private const OkdeskNotificationType _notificationType = OkdeskNotificationType.DeadlineUpdated;

    public Task Handle(IssueDeadlineUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var commentTemplate = notificationProvider.GetNotificationTemplate(_notificationType);
        var commentContent = commentTemplate
            .Replace("[contact]", notification.ContactFullName)
            .Replace("[deadline_at]", notification.DeadlineAt.ToRussianStdDateTime());

        var postCommentRequest = new PostIssueCommentRequest(notification.IssueId, commentContent, notification.AssigneeId);
        return okdeskService.PostCommentAsync(postCommentRequest, cancellationToken);
    }
}
