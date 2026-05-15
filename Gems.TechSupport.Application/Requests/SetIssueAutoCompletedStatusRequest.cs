using Gems.TechSupport.Domain.Enums;
using System.Text.Json.Serialization;

namespace Gems.TechSupport.Application.Requests;

public record SetIssueAutoCompletedStatusRequest(
    long IssueId,
    IssueStatus IssueStatus,
    string Comment,
    string SolutionTemplate,
    string TimeSpentTemplate)
{
    [JsonIgnore]
    public long IssueId { get; init; } = IssueId;

    [JsonPropertyName("code")]
    public string StatusCode { get; init; } = IssueStatus.ToString().ToLower();

    [JsonPropertyName("comment")]
    public string Comment { get; init; } = Comment;

    [JsonPropertyName("comment_public")]
    public bool IsPublic { get; init; } = false;

    [JsonPropertyName("custom_parameters")]
    public SolutionParameters CustomParameters { get; init; } = new(SolutionTemplate);

    [JsonPropertyName("time_entry")]
    public List<TimeEntry> TimeParameters { get; init; } = new() { new TimeEntry(TimeSpentTemplate) };
}

public class SolutionParameters(string Solution)
{
    [JsonPropertyName("solution")]
    public string Solution { get; init; } = Solution;
}

public class TimeEntry(string timeSpentTemplate)
{
    [JsonPropertyName("formatted_spent_time")]
    public string TimeSpent { get; init; } = timeSpentTemplate;
}
