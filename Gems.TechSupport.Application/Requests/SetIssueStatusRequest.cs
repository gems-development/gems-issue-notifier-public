using Gems.TechSupport.Domain.Enums;
using System.Text.Json.Serialization;

namespace Gems.TechSupport.Application.Requests;

public record SetIssueStatusRequest(long IssueId, IssueStatus IssueStatus)
{
    [JsonIgnore]
    public long IssueId { get; init; } = IssueId;

    [JsonPropertyName("code")]
    public string StatusCode { get; init; } = IssueStatus.ToString().ToLower();
}
