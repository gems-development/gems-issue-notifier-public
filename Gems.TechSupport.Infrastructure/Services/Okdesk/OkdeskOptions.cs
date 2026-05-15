using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Domain.Enums;

namespace Gems.TechSupport.Infrastructure.Services.Okdesk;

public class OkdeskOptions
{
    public const string ConfigurationSection = "Okdesk";

    public required string BaseAddress { get; init; } = string.Empty;
    public required string ApiToken { get; init; } = string.Empty;
    public required string Fields { get; init; } = string.Empty;
    public required int RequestsPerSecondLimit { get; init; }
    public required int IssuesRequestIntervalInMinutes { get; init; }
    public required IReadOnlyCollection<string> FilterByCompanyIds { get; init; }
    public required IReadOnlyCollection<string> FilterByAssigneeIds { get; init; }
    public required IReadOnlyCollection<string> FilterByContactIds { get; init; }
    public required IReadOnlyCollection<string> FilterByStatutes { get; init; }
    public required string HeaderForCommentTemplateWithoutContact { get; init; } = string.Empty;
    public required string HeaderForCommentTemplateWithContact { get; init; } = string.Empty;
    public required Dictionary<OkdeskNotificationType, string> MessageTemplates { get; init; }
    public required Dictionary<IssueStatus, string> StatusUpdatedMessageTemplates { get; init; }
    public required List<KeyValuePair<string, string>> ProblemCommentTemplates { get; init; }
}

public class KeyValuePair<K, V>
{
    public K Key { get; set; }
    public V Value { get; set; }
}
