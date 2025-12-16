using Gems.TechSupport.Application.Responses.Models;
using System.Text.Json.Serialization;

namespace Gems.TechSupport.Application.Responses.Webhooks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "event_type")]
[JsonDerivedType(typeof(PriorityUpdatedWebhookEvent), "new_priority")]
public abstract class WebhookEvent
{
    [JsonPropertyName("author")]
    public required WebhookEventAuthorInfo Author { get; init; }
}

public class PriorityUpdatedWebhookEvent : WebhookEvent
{
    [JsonPropertyName("old_priority")]
    public required PriorityResponse OldPriority { get; init; }

    [JsonPropertyName("new_priority")]
    public required PriorityResponse NewPriority { get; init; }
}

public class WebhookEventAuthorInfo
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }
}