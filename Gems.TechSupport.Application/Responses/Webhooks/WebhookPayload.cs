using Gems.TechSupport.Application.Responses.Models;
using System.Text.Json.Serialization;

namespace Gems.TechSupport.Application.Responses.Webhooks;

public class WebhookPayload
{
    [JsonPropertyName("event")]
    public required WebhookEvent WebhookEvent { get; init; }

    [JsonPropertyName("issue")]
    public required IssueResponse IssueData { get; init; }
}
