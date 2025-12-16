using Gems.TechSupport.Application.Abstractions.Okdesk;
using Microsoft.Extensions.Options;

namespace Gems.TechSupport.Infrastructure.Services.Okdesk;

public class OkdeskNotificationTemplatesProvider(IOptionsMonitor<OkdeskOptions> options) 
    : IOkdeskNotificationTemplatesProvider
{
    public string GetNotificationTemplate(OkdeskNotificationType notificationType)
    {
        var okdeskOptions = options.CurrentValue;

        return notificationType switch
        {
            OkdeskNotificationType.DeadlineUpdated => okdeskOptions.MessageTemplates[OkdeskNotificationType.DeadlineUpdated],
            OkdeskNotificationType.StatusUpdated => okdeskOptions.MessageTemplates[OkdeskNotificationType.StatusUpdated],
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
}
