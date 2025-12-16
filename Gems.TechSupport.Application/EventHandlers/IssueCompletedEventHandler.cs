using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.EventHandlers;

internal sealed class IssueCompletedEventHandler(
    IOkdeskNotificationTemplatesProvider notificationProvider,
    IOkdeskService okdeskService)
    : IDomainEventHandler<IssueCompletedEvent>
{
    private const OkdeskNotificationType _notificationType = OkdeskNotificationType.IssueCompleted;

    public Task Handle(IssueCompletedEvent notification, CancellationToken cancellationToken)
    {
        var commentContent = notificationProvider.GetNotificationTemplate(_notificationType);

        var postCommentRequest = new PostIssueCommentRequest(notification.IssueId, commentContent, notification.AssigneeId);
        return okdeskService.PostCommentAsync(postCommentRequest, cancellationToken);
    }
}
