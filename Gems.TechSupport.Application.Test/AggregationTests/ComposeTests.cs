using Gems.TechSupport.Application.Abstractions.Aggregation;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Application.Test.HelperMethods;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Primitives;
using Gems.TechSupport.Infrastructure.Services.Aggregation;
using Gems.TechSupport.Infrastructure.Services.Okdesk;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Gems.TechSupport.Application.Test.AggregationTests;


[TestFixture]
public class ComposeTests()
{
    private CommentComposer _sut = null!;
    private Mock<IOkdeskService> okdeskService = null!;
    private Mock<IOkdeskNotificationTemplatesProvider> provider = null!;
    private Mock<IOptionsMonitor<OkdeskOptions>> options = null!;
    private Mock<ILogger<CommentComposer>> logger = null!;
    private CancellationToken _ct;
    [SetUp]
    public void SetUp()
    {
        okdeskService = new Mock<IOkdeskService>();
        provider = new Mock<IOkdeskNotificationTemplatesProvider>();
        options = new Mock<IOptionsMonitor<OkdeskOptions>>();
        logger = new Mock<ILogger<CommentComposer>>();
        _ct = CancellationToken.None;

        _sut = new CommentComposer(okdeskService.Object, provider.Object, options.Object, logger.Object);
    }
    [Test]
    public async Task Compose_WhenIssueHaveAggregatePlan_ShouldCreatePostCommentBody()
    {
        // Arrange
        var commentBody = "Добрый день, ContactName.<br></br>\r\nПриступили к работе над заявкой. Свяжемся с вами сразу же, как появится актуальная информация о решении или возникнут дополнительные вопросы.<br></br>\r\nМы передали вашу заявку в команду тестирования. Вероятно, это дефект, требующий исправления и обновления системы.\nСрок решения по заявке будет увеличен. Пожалуйста, ожидайте.<br></br>\r\n";

        var collections = new List<IDomainEvent>
        {
            new IssueDeadlineNotificationEvent(1,10,"ContactName", IssueType.Service, IssuePriority.Low),
            new IssueStatusUpdatedEvent(1,10,"ContactName", IssueStatus.InWork, IssueStatus.Testirovanie)
        };

        var plan = new CommentAggregatePlan(1, 10, "ContactName", true, collections);

        OkdeskOptionsSetup.SetupOptions(options);

        ProviderSetup.SetupOkdeskNotification(provider, OkdeskNotificationType.Service);
        ProviderSetup.SetupOkdeskNotification(provider, IssueStatus.Testirovanie);
        ProviderSetup.SetupOkdeskNotification(provider, "ContactName");

        // Act
        await _sut.Compose(plan, _ct);

        // Assert
        okdeskService.Verify(o => o.PostCommentAsync(
                It.Is<PostIssueCommentRequest>(p =>
                    p.IssueId == plan.IssueId &&
                    p.Content.Replace("\r\n", "\n").Contains(commentBody.Replace("\r\n", "\n")) &&
                    p.AuthorId == plan.AssigneId
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once
        );
    }
    [Test]
    public async Task Compose_WhenIssueHave_ShouldCreateCommenBodyWith()
    {
        // Arrange
        var commentBody = "Добрый день.<br></br>\r\nПриступили к работе над заявкой. Свяжемся с вами сразу же, как появится актуальная информация о решении или возникнут дополнительные вопросы.<br></br>\r\nПосле рассмотрения вашей заявки мы изменили ее приоритет на средний, так как описанная проблема не влияет на корректную работу основных функций.<br></br>\r\n";

        var collections = new List<IDomainEvent>
        {
            new IssueDeadlineNotificationEvent(1,10,"ContactName", IssueType.Incident, IssuePriority.Normal),
            new IssuePriorityUpdatedEvent(1,10,"ContactName", IssuePriority.High, IssuePriority.Normal, "employee")
        };

        var plan = new CommentAggregatePlan(1, 10, "", true, collections);

        OkdeskOptionsSetup.SetupOptions(options);

        ProviderSetup.SetupOkdeskNotification(provider, OkdeskNotificationType.IncidentNormal);
        ProviderSetup.SetupOkdeskNotification(provider, OkdeskNotificationType.PriorityUpdatedToNormal);
        ProviderSetup.SetupOkdeskNotification(provider, "");

        // Act
        await _sut.Compose(plan, _ct);

        // Assert
        okdeskService.Verify(o => o.PostCommentAsync(
                It.Is<PostIssueCommentRequest>(p =>
                    p.IssueId == plan.IssueId &&
                    p.Content.Replace("\r\n", "\n").Contains(commentBody.Replace("\r\n", "\n")) &&
                    p.AuthorId == plan.AssigneId
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once
        );
    }
}
