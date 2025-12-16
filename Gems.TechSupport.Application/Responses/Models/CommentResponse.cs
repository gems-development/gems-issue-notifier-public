using System.Text.Json.Serialization;

namespace Gems.TechSupport.Application.Responses.Models;

public record CommentResponse
{
    public long Id { get; init; }
    public string Content { get; init; } = string.Empty;
    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; init; }
    public required CommentAuthorResponse Author { get; init; }
}
