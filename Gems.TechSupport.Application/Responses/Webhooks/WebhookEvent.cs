using Gems.TechSupport.Application.Responses.Models;
using System.Text.Json.Serialization;
namespace Gems.TechSupport.Application.Responses.Webhooks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "event_type")]
[JsonDerivedType(typeof(PriorityUpdatedWebhookEvent), "new_priority")]
[JsonDerivedType(typeof(StatusUpdatedWebhookEvent), "new_ticket_status")]
[JsonDerivedType(typeof(ProblemUpdatedWebhookEvent), "change_ticket_parameter")]
[JsonDerivedType(typeof(TypeUpdatedWebhookEvent), "update_issue_work_type")]

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

public class StatusUpdatedWebhookEvent : WebhookEvent
{
    [JsonPropertyName("old_status")]
    public required StatusResponse OldStatus { get; init; }

    [JsonPropertyName("new_status")]
    public required StatusResponse NewStatus { get; init; }
}
public class TypeUpdatedWebhookEvent : WebhookEvent
{
    [JsonPropertyName("old_type")]
    public required TypeResponse OldType { get; init; }

    [JsonPropertyName("new_type")]
    public required TypeResponse NewType { get; init; }
}

public class ProblemUpdatedWebhookEvent : WebhookEvent
{
    [JsonPropertyName("changed_parameters")]
    public required List<WebhookEventProblemParameters> Parameters { get; init; }
}

public class WebhookEventProblemParameters
{
    [JsonPropertyName("code")]
    public required string? Code { get; init; }

    [JsonPropertyName("before")]
    public required string? OldProblem { get; init; }

    [JsonPropertyName("after")]
    public required string? NewProblem { get; init; }
}

public class WebhookEventAuthorInfo
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }
}
