using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Gems.TechSupport.Application.Responses.Models;

public record IssueResponse
{
    [JsonIgnore]
    private const string SkitPattern = @"\[SKIT\s*#(?<num>\d+)\]";

    public long Id { get; init; }
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; init; }
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; init; }
    [JsonPropertyName("deadline_at")]
    public DateTime? DeadlineAt { get; init; }
    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public StatusResponse? Status { get; init; }
    public TypeResponse? Type { get; init; }
    public PriorityResponse? Priority { get; init; }
    public CompanyResponse? Company { get; init; }
    public ContactResponse? Contact { get; init; }
    public AssigneeResponse? Assignee { get; init; }

    [JsonIgnore]
    public bool IsSkitType => Title is not null && Regex.Match(Title, SkitPattern).Success;
}

public record StatusResponse(string Code);

public record TypeResponse(string Code);

public record PriorityResponse(string Code);