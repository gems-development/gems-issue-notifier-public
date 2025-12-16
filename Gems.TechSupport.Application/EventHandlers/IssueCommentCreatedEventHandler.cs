using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.EventHandlers;

internal sealed class IssueCommentCreatedEventHandler(ITelegramService telegramService) 
    : IDomainEventHandler<IssueCommentCreatedEvent>
{
    public Task Handle(IssueCommentCreatedEvent notification, CancellationToken cancellationToken)
    {
        return telegramService.SendIssueNewCommentNotificationAsync(
            notification.IssueId,
            notification.ContactFullName,
            notification.CommentContent, cancellationToken);
    }
}