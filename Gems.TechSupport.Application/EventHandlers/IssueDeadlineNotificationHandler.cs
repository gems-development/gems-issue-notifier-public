using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.EventHandlers;

internal sealed class IssueDeadlineNotificationHandler(
    IOkdeskService okdeskService,
    IOkdeskNotificationTemplatesProvider notificationProvider)
    : IDomainEventHandler<IssueDeadlineNotificationEvent>
{
    public Task Handle(IssueDeadlineNotificationEvent notification, CancellationToken cancellationToken)
    {
        var commentTemplate = GetCommentTemplate(notification.IssueType, notification.IssuePriority);

        if (string.IsNullOrWhiteSpace(commentTemplate))
        {
            return Task.CompletedTask;
        }

        var comment = commentTemplate
            .Replace("[contact]", notification.ContactFullName);

        var postCommentRequest = new PostIssueCommentRequest(notification.IssueId, comment, notification.AssigneeId);

        return okdeskService.PostCommentAsync(postCommentRequest, cancellationToken);
    }

    private string GetCommentTemplate(IssueType type, IssuePriority priority)
    {
        return (type, priority) switch
        {
            (IssueType.Consultation, _) => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.Consultation),
            (IssueType.Service, _) => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.Service),
            (IssueType.Incident, IssuePriority.Highest) => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.IncidentHighest),
            (IssueType.Incident, IssuePriority.High) => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.IncidentHigh),
            (IssueType.Incident, IssuePriority.Normal) => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.IncidentNormal),
            (IssueType.Incident, IssuePriority.Low) => notificationProvider.GetNotificationTemplate(OkdeskNotificationType.IncidentLow),
            _ => string.Empty
        };
    }
}
