using Gems.TechSupport.Domain.Primitives;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Gems.TechSupport.Infrastructure.Metrics;

internal sealed class ProcessedDomainEventsMetrics
{
    private const string MeterName = nameof(ProcessedDomainEventsMetrics);

    private readonly Meter _meter;

    private readonly ConcurrentDictionary<string, Counter<long>> _processedDomainEventsCounter = [];

    public ProcessedDomainEventsMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);

        var assembly = typeof(IDomainEvent).Assembly;

        var domainEventNames = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IDomainEvent)))
            .Select(type => type.Name)
            .ToList();

        foreach (var domainEventName in domainEventNames)
        {
            _processedDomainEventsCounter.TryAdd(domainEventName, _meter.CreateCounter<long>($"{domainEventName}"));
        }
    }

    public void RecordDomainEventProcessedSuccessfully(string eventType)
    {
        _processedDomainEventsCounter[eventType].Add(1);
    }
}
