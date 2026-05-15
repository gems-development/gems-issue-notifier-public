using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Domain.Enums;
using Moq;
using Quartz.Util;

namespace Gems.TechSupport.Application.Test.HelperMethods;

public static class ProviderSetup
{
    private static Dictionary<OkdeskNotificationType, string> _type = new Dictionary<OkdeskNotificationType, string>
    {
        [OkdeskNotificationType.Service] = "Приступили к работе над заявкой. Свяжемся с вами сразу же, как появится актуальная информация о решении или возникнут дополнительные вопросы.",
        [OkdeskNotificationType.IncidentNormal] = "Приступили к работе над заявкой. Свяжемся с вами сразу же, как появится актуальная информация о решении или возникнут дополнительные вопросы.",
        [OkdeskNotificationType.PriorityUpdatedToNormal] = "После рассмотрения вашей заявки мы изменили ее приоритет на средний, так как описанная проблема не влияет на корректную работу основных функций.",
        [OkdeskNotificationType.PriorityUpdatedToHigh] = "Мы изменили приоритет по вашей заявке на высокий, так как вопрос не связан с полной недоступностью ГИСОГД или сбоем всех функций системы."
    };
    private static Dictionary<IssueStatus, string> _status = new Dictionary<IssueStatus, string>
    {
        [IssueStatus.InWork] = "",
        [IssueStatus.Testirovanie] = "Мы передали вашу заявку в команду тестирования. Вероятно, это дефект, требующий исправления и обновления системы.\nСрок решения по заявке будет увеличен. Пожалуйста, ожидайте.",
    };

    public static void SetupOkdeskNotification(Mock<IOkdeskNotificationTemplatesProvider> provider, OkdeskNotificationType type)
    {
        provider.Setup(p => p.GetNotificationTemplate(
                It.IsAny<OkdeskNotificationType>()))
            .Returns<OkdeskNotificationType>(type => _type.TryGetValue(type, out var value) ? value : "");
    }
    public static void SetupOkdeskNotification(Mock<IOkdeskNotificationTemplatesProvider> provider, IssueStatus status)
    {
        provider.Setup(p => p.GetStatusUpdatedTemplate(
                It.IsAny<IssueStatus>()))
            .Returns<IssueStatus>(status => _status.TryGetValue(status, out var value) ? value : "");
    }
    public static void SetupOkdeskNotification(Mock<IOkdeskNotificationTemplatesProvider> provider, string displayContactName)
    {
        if (displayContactName.IsNullOrWhiteSpace())
            provider.Setup(p => p.GetHeaderTemplate(
                    It.Is<String>(x => x.Equals(""))))
                .Returns("Добрый день." + "<br></br>");
        else provider.Setup(p => p.GetHeaderTemplate(
                It.Is<String>(x => x.Equals(displayContactName))))
            .Returns("Добрый день, [contact].".Replace("[contact]", displayContactName) + "<br></br>");
    }
}
