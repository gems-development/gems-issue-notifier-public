using Gems.TechSupport.Domain.Primitives;
using Gems.TechSupport.Persistence.Outbox;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.FeatureManagement;
using Newtonsoft.Json;

namespace Gems.TechSupport.Persistence.Interceptors;

internal sealed class DomainEventsInterceptor(IFeatureManager featureManager) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            await base.SavingChangesAsync(eventData, result, cancellationToken);
            return result;
        }
        else
        {
            var domainEvents = dbContext.ChangeTracker
                .Entries<AggregateRoot>()
                .Select(x => x.Entity)
                .SelectMany(aggregateRoot =>
                {
                    var domainEvents = aggregateRoot.DomainEvents;

                    return domainEvents;
                });

            var enabledDomainEvents = await SelectEnabledDomainEvents(domainEvents);

            var outboxMessages = enabledDomainEvents
                .Select(domainEvent => new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccuredOnUtc = DateTime.UtcNow,
                    Type = domainEvent.GetType().Name,
                    Content = JsonConvert.SerializeObject(
                        domainEvent,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        })
                })
                .ToList();

            foreach (var aggregateRoot in dbContext.ChangeTracker.Entries<AggregateRoot>())
            {
                aggregateRoot.Entity.ClearDomainEvents();
            }

            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);

            await base.SavingChangesAsync(eventData, result, cancellationToken);
            return result;
        }
    }

    private async Task<IEnumerable<IDomainEvent>> SelectEnabledDomainEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        var enabledDomainEvents = new List<IDomainEvent>();

        foreach (var domainEvent in domainEvents)
        {
            if (await IsFeatureEnabledAsync(domainEvent))
            {
                enabledDomainEvents.Add(domainEvent);
            }
        }

        return enabledDomainEvents;
    }

    private Task<bool> IsFeatureEnabledAsync(IDomainEvent domainEvent)
    {
        return featureManager.IsEnabledAsync(domainEvent.GetType().Name + "Enabled");
    }
}
