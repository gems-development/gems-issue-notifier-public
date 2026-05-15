using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Primitives;
using Gems.TechSupport.Infrastructure.Services.Aggregation;
using Moq;
using NUnit.Framework;

namespace Gems.TechSupport.Application.Test.AggregationTests;

[TestFixture]
public class AggregatePlanTests()
{
    private Mock<IDisplayNameService> displayName = null!;
    private IssueCommentAggregatePlanBuilder _sut = null!;
    [SetUp]
    public void SetUp()
    {
        displayName = new Mock<IDisplayNameService>();
        _sut = new IssueCommentAggregatePlanBuilder(displayName.Object);
    }
    [Test]
    public void PlanBuilder_WhenIssueHaveDeadlineNotificationEvent_ShouldCreatePlanWithDeadlineNotifEvent()
    {
        // Arrange
        long issueId = 1;
        long assigneId = 10;

        var eventCollections = new List<IDomainEvent>
        {
            new IssueDeadlineNotificationEvent(
                issueId,
                assigneId,
                "ContactName",
                Domain.Enums.IssueType.Incident,
                Domain.Enums.IssuePriority.Low),
        };

        // Act
        var result = _sut.Build(issueId, eventCollections);

        // Assert
        Assert.That(result.IssueId, Is.EqualTo(1));
        Assert.That(result.AssigneId, Is.EqualTo(10));

        Assert.That(result.ContactDisplayName, Is.Null);

        Assert.That(result.DomainEvents.Any(x => x is IssueDeadlineNotificationEvent), Is.True);
    }
    [Test]
    public void PlanBuilder_WhenIssueHaveCompletedEvent_ShouldCreatePlanWithoutDeadlineEvents()
    {
        // Arrange
        long issueId = 1;
        long assigneId = 10;

        var eventCollections = new List<IDomainEvent>
        {
            new IssueDeadlineNotificationEvent(
                issueId,
                assigneId,
                "ContactName",
                Domain.Enums.IssueType.Incident,
                Domain.Enums.IssuePriority.Low),
            new IssueDeadlineUpdatedEvent(
                issueId,
                assigneId,
                "ContactName",
                new DateTime(2026-04-13)),
            new IssueCompletedEvent(
                issueId,
                assigneId),
        };

        displayName
            .Setup(d => d.GetDisplayName("ContactName"))
            .Returns("ContactName");

        // Act
        var result = _sut.Build(issueId, eventCollections);

        // Assert
        Assert.That(result.DomainEvents.Count(), Is.EqualTo(1));

        Assert.That(result.DomainEvents.Any(x => x is IssueDeadlineNotificationEvent), Is.False);
        Assert.That(result.DomainEvents.Any(x => x is IssueDeadlineUpdatedEvent), Is.False);

        Assert.That(result.DomainEvents, Has.Exactly(1).TypeOf<IssueCompletedEvent>());

        displayName.Verify(x => x.GetDisplayName(It.IsAny<String>()), Times.Never);
        Assert.That(result.ContactDisplayName, Is.EqualTo(null));
    }
    [Test]
    public void PlanBuilder_WhenIssueHaveProblemPostEvent_ShouldCreatePlanWithoutAnotherEvents()
    {
        // Arrange
        long issueId = 1;
        long assigneId = 10;

        var eventCollections = new List<IDomainEvent>
        {
            new IssueProblemPostCommentEvent(
                issueId,
                assigneId,
                "Проблема"),
            new IssueCompletedEvent(
                issueId,
                assigneId),
            new IssueDeadlineNotificationEvent(
                issueId,
                assigneId,
                "ContactName",
                Domain.Enums.IssueType.Incident,
                Domain.Enums.IssuePriority.Low),
            new IssueDeadlineUpdatedEvent(
                issueId,
                assigneId,
                "ContactName",
                new DateTime(2026-04-13)),
        };

        displayName
            .Setup(d => d.GetDisplayName("ContactName"))
            .Returns("ContactName");

        // Act
        var result = _sut.Build(issueId, eventCollections);

        // Assert
        displayName.Verify(d => d.GetDisplayName(It.IsAny<String>()), Times.Never);

        Assert.That(result.DomainEvents.Count(), Is.EqualTo(2));

        Assert.That(result.DomainEvents.Any(x => x is IssueDeadlineUpdatedEvent), Is.False);
        Assert.That(result.DomainEvents.Any(x => x is IssueDeadlineNotificationEvent), Is.False);

        Assert.That(result.DomainEvents, Has.Exactly(1).TypeOf<IssueProblemPostCommentEvent>());
        Assert.That(result.DomainEvents, Has.Exactly(1).TypeOf<IssueCompletedEvent>());
    }
    [Test]
    public void PlanBuilder_WhenIssueHaveIdenticEvents_ShouldCreatePlanWithLastEvent()
    {
        // Arrange
        long issueId = 1;
        long assigneId = 10;

        var eventCollections = new List<IDomainEvent>
        {
           new IssueStatusUpdatedEvent(
            issueId,
            assigneId,
            "ContactName",
            Domain.Enums.IssueStatus.InWork,
            Domain.Enums.IssueStatus.Completed
           ),
           new IssueStatusUpdatedEvent(
            issueId,
            assigneId,
            "ContactName",
            Domain.Enums.IssueStatus.Completed,
            Domain.Enums.IssueStatus.InWork
           ),
           new IssueStatusUpdatedEvent(
            issueId,
            assigneId,
            "ContactName",
            Domain.Enums.IssueStatus.InWork,
            Domain.Enums.IssueStatus.Development
           )
        };
        var lastEvent = eventCollections.OfType<IssueStatusUpdatedEvent>().LastOrDefault();

        displayName
            .Setup(d => d.GetDisplayName("ContactName"))
            .Returns("ContactName");

        // Act
        var result = _sut.Build(issueId, eventCollections);

        // Assert
        displayName.Verify(d => d.GetDisplayName(It.IsAny<String>()), Times.Once);

        Assert.That(result.DomainEvents.Count(), Is.EqualTo(1));
        Assert.That(result.DomainEvents.First(), Is.EqualTo(lastEvent));
    }
    [Test]
    public void PlanBuilder_WhenIssueWithoutEvents_ShouldCreatePlanWithoutEvents()
    {
        // Arrange
        long issueId = 1;

        var eventCollections = new List<IDomainEvent>
        {
        };

        // Act
        var result = _sut.Build(issueId, eventCollections);

        // Assert
        Assert.That(result.DomainEvents, Is.Empty);

        displayName.Verify(d => d.GetDisplayName(It.IsAny<String>()), Times.Never);

        Assert.That(result.IssueId, Is.EqualTo(1));
        Assert.That(result.AssigneId, Is.Null);
    }
    [Test]
    public void PlanBuilder_WhenIssueHaveNonFilteredEvents_ShouldCreatePlanWithFilteredEvents()
    {
        // Arrange
        long issueId = 1;
        long assigneIdFirst = 11;
        long assigneIdSecond = 12;

        var eventCollections = new List<IDomainEvent>
        {
            new IssueStatusUpdatedEvent(
            issueId,
            assigneIdFirst,
            "ContactName",
            Domain.Enums.IssueStatus.Completed,
            Domain.Enums.IssueStatus.InWork
           ),
            new IssuePriorityUpdatedEvent(
            issueId,
            assigneIdSecond,
            "ContactName",
            Domain.Enums.IssuePriority.High,
            Domain.Enums.IssuePriority.Normal,
            "AuthorType"
           ),
        };

        displayName
            .Setup(d => d.GetDisplayName("ContactName"))
            .Returns("ContactName");

        // Act
        var result = _sut.Build(issueId, eventCollections);

        // Assert
        // IssuePriorityUpdatedEvent имеет более высокий приоритет 3, чем IssueStatusUpdatedEvent 4
        Assert.That(result.DomainEvents.First(), Is.TypeOf<IssuePriorityUpdatedEvent>());

        Assert.That(result.AssigneId, Is.EqualTo(assigneIdSecond));
        Assert.That(result.ContactDisplayName, Is.EqualTo("ContactName"));
    }
}