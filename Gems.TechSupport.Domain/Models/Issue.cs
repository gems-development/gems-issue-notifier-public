using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Primitives;
using System.Text.RegularExpressions;

namespace Gems.TechSupport.Domain.Models;

public class Issue : AggregateRoot
{
    private readonly List<Comment> _comments = [];
    private const string SkitPattern = @"\[SKIT\s*#(?<num>\d+)\]";

    private Issue(
        long id,
        string? title,
        string? description,
        IssuePriority? priority,
        IssueStatus? status,
        IssueType? type,
        DateTime? createdAt,
        DateTime? updatedAt,
        DateTime? deadlineAt,
        DateTime? completedAt,
        Company? company,
        Contact? contact,
        Assignee? assignee)
    {
        Id = id;
        Title = title;
        Description = description;
        Priority = priority;
        Status = status;
        Type = type;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeadlineAt = deadlineAt;
        CompletedAt = completedAt;
        Company = company;
        Contact = contact;
        Assignee = assignee;
    }

    protected Issue()
    {
    }

    public string? Title { get; private set; }
    public string? Description { get; set; }
    public IssuePriority? Priority { get; private set; }
    public IssueStatus? Status { get; private set; }
    public IssueType? Type { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeadlineAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public Company? Company { get; set; }
    public Contact? Contact { get; set; }
    public Assignee? Assignee { get; set; }
    public DateTime? StaleNotifiedAt { get; private set; }


    public bool IsSkitType => Title is not null && Regex.Match(Title, SkitPattern).Success;

    public void MarkAsStaleNotified() => StaleNotifiedAt = DateTime.UtcNow;

    /// <summary>
    /// Recreates an existing issue from the Okdesk API without triggering creation events.
    /// This method should be used to add new issues that were created outside the specified time interval,
    /// for which the issue list is requested.
    /// Does not generate domain events for notifications about creation as this represents an existing entity.
    /// </summary>
    public static Issue CreateExisting(
        long id,
        string? title = null,
        string? description = null,
        IssuePriority? priority = null,
        IssueStatus? status = null,
        IssueType? type = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? deadlineAt = null,
        DateTime? completedAt = null,
        Company? company = null,
        Contact? contact = null,
        Assignee? assignee = null,
        List<Comment>? comments = null)
    {
        return Create(id, false, title, description, priority, status, type, createdAt, updatedAt,
            deadlineAt, completedAt, company, contact, assignee, comments);
    }

    /// <summary>
    /// Creates a new issue in the system with domain events for notifications.
    /// This method should be used when the issue was created in Okdesk system within the specified time interval during which
    /// the list of issues is requested.
    /// Triggers deadline notification events for assignees when all required data is provided.
    /// </summary>
    public static Issue CreateNew(
        long id,
        string? title = null,
        string? description = null,
        IssuePriority? priority = null,
        IssueStatus? status = null,
        IssueType? type = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? deadlineAt = null,
        DateTime? completedAt = null,
        Company? company = null,
        Contact? contact = null,
        Assignee? assignee = null,
        List<Comment>? comments = null)
    {
        return Create(id, true, title, description, priority, status, type, createdAt, updatedAt,
            deadlineAt, completedAt, company, contact, assignee, comments);
    }

    public void Update(Issue issue)
    {
        Title = UpdateIfNotNull(Title, issue.Title);
        Type = UpdateIfNotNull(Type, issue.Type);
        Description = UpdateIfNotNull(Description, issue.Description);
        UpdatedAt = UpdateIfNotNull(UpdatedAt, issue.UpdatedAt);
        Assignee = UpdateIfNotNull(Assignee, issue.Assignee);
        Contact = UpdateIfNotNull(Contact, issue.Contact);
        Company = UpdateIfNotNull(Company, issue.Company);

        UpdateStatus(issue);
        UpdateDeadline(issue);

        foreach (var comment in issue._comments)
        {
            if (!_comments.Contains(comment))
            {
                AddComment(comment);
                if (CheckStatusToDisableEventsGeneration())
                {
                    var issueCommentCreatedEvent = new IssueCommentCreatedEvent(Id, Assignee?.Id, comment.Contact.Id,
                     comment.Contact.FullName, comment.Public, comment.Content);
                    AddDomainEvent(issueCommentCreatedEvent);
                }
            }
        }
    }

    public void UpdatePriority(Issue issue, string updateAuthorType)
    {
        if (issue.Priority is not null && issue.Priority != Priority)
        {
            var oldPriority = Priority;
            Priority = issue.Priority;

            if (CheckStatusToDisableEventsGeneration() && Assignee is not null && Contact is not null && IsSkitType is false && oldPriority is not null)
            {
                var priorityUpdatedEvent = new IssuePriorityUpdatedEvent(
                    Id, Assignee.Id, Contact.FullName, oldPriority.Value, issue.Priority.Value, updateAuthorType);

                AddDomainEvent(priorityUpdatedEvent);
            }
        }
    }
    public void AddComment(Comment comment)
    {
        _comments.Add(comment);
    }

    private static Issue Create(
        long id,
        bool newCreated = false,
        string? title = null,
        string? description = null,
        IssuePriority? priority = null,
        IssueStatus? status = null,
        IssueType? type = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? deadlineAt = null,
        DateTime? completedAt = null,
        Company? company = null,
        Contact? contact = null,
        Assignee? assignee = null,
        List<Comment>? comments = null)
    {
        var issue = new Issue(id, title, description, priority, status, type, createdAt,
            updatedAt, deadlineAt, completedAt, company, contact, assignee);

        if (comments is not null)
        {
            issue._comments.AddRange(comments);
        }

        return issue;
    }

    public void UpdateStatus(Issue issue)
    {
        if (issue.Status is not null && issue.Status != Status)
        {
            var oldStatus = Status;
            var canGenerateEvents = CheckStatusToDisableEventsGeneration();
            Status = issue.Status;

            if (issue.CompletedAt is not null && issue.CompletedAt != CompletedAt)
            {
                CompletedAt = issue.CompletedAt;

                if (canGenerateEvents)
                {
                    if (Status == IssueStatus.Completed &&
                       Assignee is not null && IsSkitType is false)
                    {
                        var issueCompletedEvent = new IssueCompletedEvent(Id, Assignee.Id);
                        AddDomainEvent(issueCompletedEvent);
                    }
                    else if (Status == IssueStatus.Wish &&
                        Assignee is not null && IsSkitType is false)
                    {
                        var statusUpdatedEvent = new IssueStatusUpdatedEvent(
                        Id, Assignee.Id, Contact.FullName, oldStatus!.Value, issue.Status.Value);

                        AddDomainEvent(statusUpdatedEvent);
                    }
                }
            }
            else if (canGenerateEvents && oldStatus == IssueStatus.Opened && issue.Status == IssueStatus.InWork)
            {
                if (Assignee is not null && Contact is not null && Type is not null && Priority is not null && IsSkitType is false)
                {
                    var deadlineNotificationEvent = new IssueDeadlineNotificationEvent(
                        Id, Assignee.Id, Contact.FullName, Type.Value, Priority.Value);

                    AddDomainEvent(deadlineNotificationEvent);
                }
            }
            else if (canGenerateEvents && Assignee is not null && Contact is not null && IsSkitType is false && oldStatus is not null)
            {
                var statusUpdatedEvent = new IssueStatusUpdatedEvent(
                    Id, Assignee.Id, Contact.FullName, oldStatus!.Value, issue.Status.Value);

                AddDomainEvent(statusUpdatedEvent);
            }
        }
    }

    public void UpdateProblem(string? newProblemName)
    {
        if (!CheckStatusToDisableEventsGeneration() ||
            string.IsNullOrWhiteSpace(newProblemName) ||
            Assignee is null ||
            IsSkitType)
        { return; }

        Status = IssueStatus.Completed;

        if (Assignee is not null && IsSkitType is false)
        {
            var issueAutoCompletedEvent = new IssueAutoCompletedEvent(Id, Assignee.Id, newProblemName);
            AddDomainEvent(issueAutoCompletedEvent);
            var issueCompletedEvent = new IssueCompletedEvent(Id, Assignee.Id);
            AddDomainEvent(issueCompletedEvent);
            var postCommentForClosedIssue = new IssueProblemPostCommentEvent(Id, Assignee.Id, newProblemName);
            AddDomainEvent(postCommentForClosedIssue);
        }
    }

    private void UpdateDeadline(Issue issue)
    {
        if (issue.DeadlineAt is not null && issue.DeadlineAt != DeadlineAt)
        {
            DeadlineAt = issue.DeadlineAt;

            if (CheckStatusToDisableEventsGeneration() && Assignee is not null && Contact is not null && issue.DeadlineAt is not null && IsSkitType is false)
            {
                var deadlineUpdatedEvent = new IssueDeadlineUpdatedEvent(
                Id, Assignee.Id, Contact.FullName, issue.DeadlineAt.Value);

                AddDomainEvent(deadlineUpdatedEvent);
            }
        }
    }
    public void UpdateType(IssueType? newType)
    {
        if (newType is not null && newType != Type)
        {
            Type = newType;
        }
    }
    private static T UpdateIfNotNull<T>(T? newValue, T current) => newValue ?? current;


    private bool CheckStatusToDisableEventsGeneration()
    {
        return Status != IssueStatus.Completed &&
           Status != IssueStatus.Closed &&
           Status != IssueStatus.Wish;
    }
}
