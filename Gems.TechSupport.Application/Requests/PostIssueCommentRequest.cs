using System.Text.Json.Serialization;

namespace Gems.TechSupport.Application.Requests;

public record PostIssueCommentRequest(long IssueId, string Content, long AuthorId, bool Public = true)
{
    [JsonPropertyName("id")]
    public long IssueId { get; init; } = IssueId;

    [JsonPropertyName("content")]
    public string Content { get; init; } = Content;

    [JsonPropertyName("author_id")]
    public long AuthorId { get; init; } = AuthorId;

    [JsonPropertyName("public")]
    public bool Public { get; init; } = Public;
}