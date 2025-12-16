namespace Gems.TechSupport.Application.Abstractions.Okdesk;

public enum OkdeskNotificationType
{
    StatusUpdated = 1,
    IssueCompleted,
    DeadlineUpdated,
    PriorityUpdatedToHighest,
    PriorityUpdatedToHigh,
    PriorityUpdatedToNormal,
    PriorityUpdatedToLow,
    Consultation,
    Service,
    IncidentHighest,
    IncidentHigh,
    IncidentNormal,
    IncidentLow
}