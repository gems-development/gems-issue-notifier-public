namespace Gems.TechSupport.Application.Abstractions.Okdesk;

public interface IOkdeskNotificationTemplatesProvider
{
    public string GetNotificationTemplate(OkdeskNotificationType type);
}
