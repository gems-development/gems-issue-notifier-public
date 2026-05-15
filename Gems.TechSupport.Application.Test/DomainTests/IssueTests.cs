using Gems.TechSupport.Application.Test.HelperMethods;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Models;
using NUnit.Framework;

namespace Gems.TechSupport.Application.Test.DomainTests;

public class IssueTests
{
    private static readonly object[] IsSkitTypeCases =
    {
        new object[] { "[SKIT #123] Test issue", true },
        new object[] { "[SKIT#123] Test issue", true },
        new object[] { "[SKIT   #123] Test issue", true },
        new object[] { "Regular issue", false },
        new object[] { "[SKIT #abc] Test issue", false },
        new object[] { "[SKIT 123] Test issue", false },
        new object?[] { null, false },
    };

    private static readonly object[] PriorityChangedCases =
    {
        new object[]
        {
            new IssueBuilder()
                .WithPriority(IssuePriority.Normal)
                .WithStatus(IssueStatus.Opened)
                .BuildExisting(),
            true,
        },
        new object[]
        {
            new IssueBuilder()
                .AsSkit()
                .WithPriority(IssuePriority.Normal)
                .WithStatus(IssueStatus.Opened)
                .BuildExisting(),
            false,
        },
    };
    private static readonly object[] StatusChangedCases =
    {
        new object[]
        {
            new IssueBuilder()
                .WithPriority(IssuePriority.Normal)
                .WithStatus(IssueStatus.Opened)
                .BuildExisting(),
            true,
        },
        new object[]
        {
            new IssueBuilder()
                .AsSkit()
                .WithPriority(IssuePriority.Normal)
                .WithStatus(IssueStatus.Opened)
                .BuildExisting(),
            false,
        },
    };
    private static readonly object[] DeadlineChangedCases =
    {
        new object[]
        {
            new IssueBuilder()
                .WithDeadlineAt(new DateTime(2026, month: 1, 1, 9, 0, 0, DateTimeKind.Utc))
                .WithStatus(IssueStatus.InWork)
                .BuildExisting(),
            true,
        },
        new object[]
        {
            new IssueBuilder()
                .AsSkit()
                .WithDeadlineAt(new DateTime(2026, month: 1, 1, 9, 0, 0, DateTimeKind.Utc))
                .WithStatus(IssueStatus.InWork)
                .BuildExisting(),
            false,
        },
    };
    private static readonly object[] ProblemChangedCases =
        {
        new object[]
        {
            new IssueBuilder()
                .WithStatus(IssueStatus.InWork)
                .BuildExisting(),
            true,
        },
        new object[]
        {
            new IssueBuilder()
                .AsSkit()
                .WithStatus(IssueStatus.InWork)
                .BuildExisting(),
            false,
        },
    };
    [TestCaseSource(nameof(IsSkitTypeCases))]
    public void Issue_IsSkitType_ShouldReturnExpectedResult(string? title, bool expected)
    {
        //arrange
        var issue = new IssueBuilder()
            .WithTitle(title)
            .BuildExisting();
        //assert
        Assert.That(issue.IsSkitType, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(PriorityChangedCases))]
    public void Issue_WhenPriorityChanged_ShouldUpdatePriority(Issue existingIssue, bool shouldCreateEvent)
    {
        //arrange
        var incomingIssue = new IssueBuilder()
            .WithPriority(IssuePriority.Low)
            .BuildExisting();

        //act
        existingIssue.UpdatePriority(incomingIssue, "employee");

        //assert
        Assert.That(existingIssue.Priority, Is.EqualTo(incomingIssue.Priority));

        var events = existingIssue.DomainEvents.OfType<IssuePriorityUpdatedEvent>().ToList();
        Assert.That(events.Count, Is.EqualTo(shouldCreateEvent ? 1 : 0));

        if (shouldCreateEvent)
        {
            var domainEvent = events.Single();
            Assert.That(domainEvent.OldPriority, Is.EqualTo(IssuePriority.Normal));
            Assert.That(domainEvent.NewPriority, Is.EqualTo(IssuePriority.Low));
        }
    }
    [TestCaseSource(nameof(StatusChangedCases))]
    public void Issue_WhenStatusChanged_ShouldUpdateStatus(Issue existingIssue, bool shouldCreateEvent)
    {
        //arrange 
        var incomingIssue = new IssueBuilder()
            .WithStatus(IssueStatus.InWork)
            .BuildExisting();

        //act
        existingIssue.UpdateStatus(incomingIssue);

        //assert
        var events = existingIssue.DomainEvents.OfType<IssueDeadlineNotificationEvent>().ToList();
        Assert.That(events.Count, Is.EqualTo(shouldCreateEvent ? 1 : 0));

        if (shouldCreateEvent)
        {
            Assert.That(existingIssue.Status, Is.EqualTo(incomingIssue.Status));
        }
    }
    [TestCaseSource(nameof(DeadlineChangedCases))]
    public void Issue_WhenDeadlineChanged_ShouldUpdateDeadline(Issue existingIssue, bool shouldCreateEvent)
    {
        //arrange 
        var incomingIssue = new IssueBuilder()
        .WithDeadlineAt(new DateTime(2026, month: 1, 1, 10, 0, 0, DateTimeKind.Utc))
        .WithStatus(IssueStatus.InWork)
        .BuildExisting();
        //act
        existingIssue.Update(incomingIssue);

        //assert
        var events = existingIssue.DomainEvents.OfType<IssueDeadlineUpdatedEvent>().ToList();
        Assert.That(events.Count, Is.EqualTo(shouldCreateEvent ? 1 : 0));
        if (shouldCreateEvent)
        {
            Assert.That(existingIssue.DeadlineAt, Is.EqualTo(incomingIssue.DeadlineAt));
            Assert.That(existingIssue.DomainEvents.Any(x => x is IssueStatusUpdatedEvent), Is.False);
            Assert.That(existingIssue.DomainEvents, Has.Exactly(1).TypeOf<IssueDeadlineUpdatedEvent>());
        }
    }
    [TestCaseSource(nameof(ProblemChangedCases))]
    public void Issue_WhenProblemChanged_ShouldUpdateProblem(Issue existingIssue, bool shouldCreateEvent)
    {
        //arrange 
        var problemName = "Problem test";
        //act
        existingIssue.UpdateProblem(problemName);

        //assert
        var events = existingIssue.DomainEvents.ToList();
        Assert.That(events.Count, Is.EqualTo(shouldCreateEvent ? 3 : 0));
        if (shouldCreateEvent)
        {
            Assert.That(existingIssue.Status, Is.EqualTo(IssueStatus.Completed));

            Assert.That(existingIssue.DomainEvents.Count, Is.EqualTo(3));
            Assert.That(existingIssue.DomainEvents, Has.Exactly(1).TypeOf<IssueAutoCompletedEvent>());
            Assert.That(existingIssue.DomainEvents, Has.Exactly(1).TypeOf<IssueCompletedEvent>());
            Assert.That(existingIssue.DomainEvents, Has.Exactly(1).TypeOf<IssueProblemPostCommentEvent>());
        }
    }
}
