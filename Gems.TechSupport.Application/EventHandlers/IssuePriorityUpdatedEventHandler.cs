using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.EventHandlers;

internal sealed class IssuePriorityUpdatedEventHandler(
    ITelegramService telegramService)
    : IDomainEventHandler<IssuePriorityUpdatedEvent>
{
    public Task Handle(IssuePriorityUpdatedEvent notification, CancellationToken cancellationToken)
    {
        return telegramService.SendIssuePriorityUpdatedNotificationAsync(
            notification.IssueId,
            notification.AssigneeId,
            notification.ContactFullName,
            notification.NewPriority, cancellationToken);
    }
}