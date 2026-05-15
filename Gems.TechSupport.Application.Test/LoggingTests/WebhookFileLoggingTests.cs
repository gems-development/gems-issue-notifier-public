using Gems.TechSupport.Application.Commands.Okdesk;
using Gems.TechSupport.Application.Commands.Okdesk.Logging;
using Gems.TechSupport.Application.Responses.Models;
using Gems.TechSupport.Application.Responses.Webhooks;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using NUnit.Framework;

namespace Gems.TechSupport.Application.Test.LoggingTests;

[TestFixture]
public class WebhookFileLoggingTests
{
    private Mock<IFeatureManager> featureManager = null!;
    private Mock<ILogger<LoggingPipelineBehavior<OkdeskWebhookProcessingCommand, MediatR.Unit>>> logger = null!;
    private RequestHandlerDelegate<MediatR.Unit> next = null!;
    private LoggingPipelineBehavior<OkdeskWebhookProcessingCommand, MediatR.Unit> _sut = null!;
    private CancellationToken ct;

    [SetUp]
    public void SetUp()
    {
        featureManager = new Mock<IFeatureManager>();
        logger = new Mock<ILogger<LoggingPipelineBehavior<OkdeskWebhookProcessingCommand, MediatR.Unit>>>();
        ct = CancellationToken.None;


        _sut = new LoggingPipelineBehavior<OkdeskWebhookProcessingCommand, MediatR.Unit>(logger.Object, featureManager.Object);
    }
    [Test]
    public async Task GivenRightOptions_ShouldLog()
    {

        var testAssignee = new Assignee { Id = 11, FullName = "Assignee" };
        var testContact = new Contact { Id = 10, FullName = "Contact" };
        var issue = Issue.CreateExisting(
                id: 10,
                title: "Test issue",
                priority: IssuePriority.Low,
                assignee: testAssignee,
                contact: testContact
            );
        var webhookEvent = new PriorityUpdatedWebhookEvent
        {
            OldPriority = new PriorityResponse(
                    issue.Priority.ToString()
                    ),
            NewPriority = new PriorityResponse(IssuePriority.High.ToString()),
            Author = new WebhookEventAuthorInfo
            {
                Id = 10,
                Type = "user"
            }
        };
        var command = new OkdeskWebhookProcessingCommand(issue, webhookEvent);

        next = (ct) => Task.FromResult(default(MediatR.Unit));

        featureManager.Setup(x => x.IsEnabledAsync("WebhookRequestLoggingEnabled")).ReturnsAsync(true);

        await _sut.Handle(command, next, ct);

        logger.Verify(
             x => x.Log(
                 LogLevel.Information,
                 It.IsAny<EventId>(),
                 It.Is<It.IsAnyType>((v, t) =>
                 v.ToString().Contains("WebhookEvent")),
                 It.IsAny<Exception>(),
                 It.IsAny<Func<It.IsAnyType, Exception, string>>()),
             Times.Once);
    }

    [Test]
    public async Task GivenNotOkdeskWebhookProcessingCommand_ShouldNotLog()
    {
        var testAssignee = new Assignee { Id = 11, FullName = "Assignee" };
        var testContact = new Contact { Id = 10, FullName = "Contact" };
        var issue = Issue.CreateExisting(
                id: 10,
                title: "Test issue",
                priority: IssuePriority.Low,
                assignee: testAssignee,
                contact: testContact
            );
        var webhookEvent = new PriorityUpdatedWebhookEvent
        {
            OldPriority = new PriorityResponse(
                    issue.Priority.ToString()
                    ),
            NewPriority = new PriorityResponse(IssuePriority.High.ToString()),
            Author = new WebhookEventAuthorInfo
            {
                Id = 10,
                Type = "user"
            }
        };
        var issueList = new List<Issue> { issue };
        var command = new OkdeskSkitIssuesCommand(issueList, 1);

        next = (ct) => Task.FromResult(default(MediatR.Unit));

        featureManager.Setup(x => x.IsEnabledAsync("WebhookRequestLoggingEnabled")).ReturnsAsync(true);


        var skitCommandLogger = new Mock<ILogger<LoggingPipelineBehavior<OkdeskSkitIssuesCommand, MediatR.Unit>>>();
        var pipelineBehavior = new LoggingPipelineBehavior<OkdeskSkitIssuesCommand, MediatR.Unit>(skitCommandLogger.Object, featureManager.Object);


        await pipelineBehavior.Handle(command, next, ct);

        skitCommandLogger.Verify(
             x => x.Log(
                 LogLevel.Information,
                 It.IsAny<EventId>(),
                 It.Is<It.IsAnyType>((v, t) =>
                 v.ToString().Contains("WebhookEvent")),
                 It.IsAny<Exception>(),
                 It.IsAny<Func<It.IsAnyType, Exception, string>>()),
             Times.Never);
    }

    [Test]
    public async Task GivenUnenabledFeature_ShouldNotLog()
    {
        var testAssignee = new Assignee { Id = 11, FullName = "Assignee" };
        var testContact = new Contact { Id = 10, FullName = "Contact" };
        var issue = Issue.CreateExisting(
                id: 10,
                title: "Test issue",
                priority: IssuePriority.Low,
                assignee: testAssignee,
                contact: testContact
            );
        var webhookEvent = new PriorityUpdatedWebhookEvent
        {
            OldPriority = new PriorityResponse(
                    issue.Priority.ToString()
                    ),
            NewPriority = new PriorityResponse(IssuePriority.High.ToString()),
            Author = new WebhookEventAuthorInfo
            {
                Id = 10,
                Type = "user"
            }
        };
        var command = new OkdeskWebhookProcessingCommand(issue, webhookEvent);

        next = (ct) => Task.FromResult(default(MediatR.Unit));

        featureManager.Setup(x => x.IsEnabledAsync("WebhookRequestLoggingEnabled")).ReturnsAsync(false);

        await _sut.Handle(command, next, ct);

        logger.Verify(
             x => x.Log(
                 LogLevel.Information,
                 It.IsAny<EventId>(),
                 It.Is<It.IsAnyType>((v, t) =>
                 v.ToString().Contains("WebhookEvent")),
                 It.IsAny<Exception>(),
                 It.IsAny<Func<It.IsAnyType, Exception, string>>()),
             Times.Never);
    }
}


