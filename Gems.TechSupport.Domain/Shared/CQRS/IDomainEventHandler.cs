using Gems.TechSupport.Domain.Primitives;
using MediatR;

namespace Gems.TechSupport.Domain.Shared.CQRS;

public interface IDomainEventHandler<TEvent> 
    : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
