using Gems.TechSupport.Domain.Enums;

namespace Gems.TechSupport.Application.Abstractions.Okdesk;

public interface IOkdeskNotificationTemplatesProvider
{
    string? GetNotificationTemplate(OkdeskNotificationType type);
    string GetStatusUpdatedTemplate(IssueStatus statusType);
    string? GetProblemTemplate(string? problemName);
    string GetHeaderTemplate(string? displayContactName);
}
