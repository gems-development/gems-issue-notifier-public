using System.Text;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Domain.Enums;
using Microsoft.Extensions.Options;
using Quartz.Util;

namespace Gems.TechSupport.Infrastructure.Services.Okdesk;

public class OkdeskNotificationTemplatesProvider(IOptionsMonitor<OkdeskOptions> options)
    : IOkdeskNotificationTemplatesProvider
{
    private readonly IOptionsMonitor<OkdeskOptions> _options = options;

    public string GetNotificationTemplate(OkdeskNotificationType notificationType)
    {
        var okdeskOptions = _options.CurrentValue;

        return notificationType switch
        {
            OkdeskNotificationType.DeadlineUpdated => okdeskOptions.MessageTemplates[OkdeskNotificationType.DeadlineUpdated],
            OkdeskNotificationType.PriorityUpdatedToHighest => okdeskOptions.MessageTemplates[OkdeskNotificationType.PriorityUpdatedToHighest],
            OkdeskNotificationType.PriorityUpdatedToHigh => okdeskOptions.MessageTemplates[OkdeskNotificationType.PriorityUpdatedToHigh],
            OkdeskNotificationType.PriorityUpdatedToNormal => okdeskOptions.MessageTemplates[OkdeskNotificationType.PriorityUpdatedToNormal],
            OkdeskNotificationType.PriorityUpdatedToLow => okdeskOptions.MessageTemplates[OkdeskNotificationType.PriorityUpdatedToLow],
            OkdeskNotificationType.IncidentHigh => okdeskOptions.MessageTemplates[OkdeskNotificationType.IncidentHigh],
            OkdeskNotificationType.IncidentHighest => okdeskOptions.MessageTemplates[OkdeskNotificationType.IncidentHighest],
            OkdeskNotificationType.IncidentNormal => okdeskOptions.MessageTemplates[OkdeskNotificationType.IncidentNormal],
            OkdeskNotificationType.IncidentLow => okdeskOptions.MessageTemplates[OkdeskNotificationType.IncidentLow],
            OkdeskNotificationType.Consultation => okdeskOptions.MessageTemplates[OkdeskNotificationType.Consultation],
            OkdeskNotificationType.IssueCompleted => okdeskOptions.MessageTemplates[OkdeskNotificationType.IssueCompleted],
            OkdeskNotificationType.Service => okdeskOptions.MessageTemplates[OkdeskNotificationType.Service],
            _ => string.Empty
        };
    }

    public string GetStatusUpdatedTemplate(IssueStatus statusType)
    {
        var okdeskOptions = _options.CurrentValue;

        return statusType switch
        {
            IssueStatus.Testirovanie => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.Testirovanie],
            IssueStatus.Vnedrenie => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.Vnedrenie],
            IssueStatus.Development => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.Development],
            IssueStatus.Request => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.Request],
            IssueStatus.Analytics => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.Analytics],
            IssueStatus.Backlog => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.Backlog],
            IssueStatus.Jira_Po => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.Jira_Po],
            IssueStatus.WaitingForUpdate => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.WaitingForUpdate],
            IssueStatus.Wish => okdeskOptions.StatusUpdatedMessageTemplates[IssueStatus.Wish],
            _ => string.Empty
        };
    }

    public string? GetProblemTemplate(string? problemName)
    {
        if (string.IsNullOrWhiteSpace(problemName))
        {
            return null;
        }
        var okdeskOptions = _options.CurrentValue;

        var templates = okdeskOptions.ProblemCommentTemplates
            .FirstOrDefault(x => string.Equals(x.Key, problemName, StringComparison.OrdinalIgnoreCase));
        var template = templates?.Value;

        return string.IsNullOrWhiteSpace(template) ? null : template;
    }
    public string GetHeaderTemplate(string? displayContactName)
    {
        var template = new StringBuilder();
        var okdeskOptions = _options.CurrentValue;
        if (!displayContactName.IsNullOrWhiteSpace())
        {
            template
                .AppendLine(okdeskOptions.HeaderForCommentTemplateWithContact + "<br></br>")
                .Replace("[contact]", displayContactName);
        }
        else
        {
            template.AppendLine(okdeskOptions.HeaderForCommentTemplateWithoutContact + "<br></br>");
        }
        return template.ToString();
    }
}
