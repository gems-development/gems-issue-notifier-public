using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.EventHandlers;

internal sealed class IssueStatusUpdatedEventHandler(
    IOkdeskNotificationTemplatesProvider notificationProvider,
    IOkdeskService okdeskService) 
    : IDomainEventHandler<IssueStatusUpdatedEvent>
{
    private const OkdeskNotificationType _notificationType = OkdeskNotificationType.StatusUpdated;

    public Task Handle(IssueStatusUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var commentTemplate = notificationProvider.GetNotificationTemplate(_notificationType);
        var commentContent = commentTemplate
            .Replace("[contact]", notification.ContactFullName)
            .Replace("[status]", TranslateStatus(notification.Status));

        var postCommentRequest = new PostIssueCommentRequest(notification.IssueId, commentContent, notification.AssigneeId);
        return okdeskService.PostCommentAsync(postCommentRequest, cancellationToken);
    }

    private string TranslateStatus(IssueStatus status)
    {
        return status switch
        {
            IssueStatus.Opened => "Новая",
            IssueStatus.InWork => "В работе",
            IssueStatus.Vnedrenie => "На внедрении",
            IssueStatus.Partner => "На партнерах",
            IssueStatus.Waiting => "Ожидание ответа",
            IssueStatus.Testirovanie => "На тестировщиках",
            IssueStatus.Analytics => "На аналитиках",
            IssueStatus.Request => "На реквестах",
            IssueStatus.Backlog => "В бэклоге",
            IssueStatus.Development => "Разработка",
            IssueStatus.WaitingForUpdate => "Ожидание обновления",
            IssueStatus.Wish => "Пожелание зафиксировано",
            IssueStatus.Delayed => "Отложена",
            IssueStatus.Completed => "Решена",
            IssueStatus.Closed => "Закрыта",
            _ => string.Empty
        };
    }
}
