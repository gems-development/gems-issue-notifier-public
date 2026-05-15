using System.Diagnostics.Metrics;

namespace Gems.TechSupport.Infrastructure.Metrics;

public sealed class ProcessedDomainEventsMetrics
{
    private const string MeterName = "ProcessedDomainEvents";
    private readonly Counter<long> _processedEvents;

    public ProcessedDomainEventsMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _processedEvents = meter.CreateCounter<long>(
            "gems.issue_notifier.processed_domain_events_total",
            description: "Number of processed domain events"
        );
    }

    public void RecordDomainEventProcessedSuccessfully(string eventType)
    {
        _processedEvents.Add(
            1,
            new KeyValuePair<string, object?>("event_type", eventType)
        );
    }
}
