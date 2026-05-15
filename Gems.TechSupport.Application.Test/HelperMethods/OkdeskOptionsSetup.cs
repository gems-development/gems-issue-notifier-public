using OkdeskPair = Gems.TechSupport.Infrastructure.Services.Okdesk.KeyValuePair<string, string>;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Infrastructure.Services.Okdesk;
using Microsoft.Extensions.Options;
using Moq;

namespace Gems.TechSupport.Application.Test.HelperMethods
{
    public static class OkdeskOptionsSetup
    {
        public static void SetupOptions(Mock<IOptionsMonitor<OkdeskOptions>> optionsOkdesk)
        {
            optionsOkdesk.Setup(o => o.CurrentValue).Returns(new OkdeskOptions
            {
                BaseAddress = "",
                ApiToken = "",
                Fields = "",
                RequestsPerSecondLimit = 0,
                IssuesRequestIntervalInMinutes = 0,
                FilterByCompanyIds = new List<string>(),
                FilterByAssigneeIds = new List<string>(),
                FilterByContactIds = new List<string>(),
                FilterByStatutes = new List<string>(),
                MessageTemplates = new Dictionary<OkdeskNotificationType, string>(),
                StatusUpdatedMessageTemplates = new Dictionary<IssueStatus, string>(),
                ProblemCommentTemplates = new List<OkdeskPair>(),
                HeaderForCommentTemplateWithContact = "Добрый день, [contact].",
                HeaderForCommentTemplateWithoutContact = "Добрый день."
            });
        }
    }
}