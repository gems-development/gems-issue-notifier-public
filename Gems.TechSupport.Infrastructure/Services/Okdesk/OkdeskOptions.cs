using Gems.TechSupport.Application.Abstractions.Okdesk;

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
    public required Dictionary<OkdeskNotificationType, string> MessageTemplates {  get; init; }
}