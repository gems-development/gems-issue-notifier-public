using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;

namespace Gems.TechSupport.Application.Test.HelperMethods;

internal sealed class IssueBuilder
{

    private long _id = 1;
    private string? _title = "Issue test";
    private string? _description = "Issue description";
    private IssuePriority? _priority = IssuePriority.Normal;
    private IssueStatus? _status = IssueStatus.Opened;
    private IssueType? _type = IssueType.Incident;
    private DateTime? _createdAt = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
    private DateTime? _updatedAt = new DateTime(2026, 1, 1, 9, 30, 0, DateTimeKind.Utc);
    private DateTime? _deadlineAt = new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc);
    private DateTime? _completedAt = null;
    private Company? _company = DomainTestFactory.CreateCompany();
    private Contact? _contact = DomainTestFactory.CreateContact();
    private Assignee? _assignee = DomainTestFactory.CreateAssignee();
    private List<Comment>? _comments = [];

    public IssueBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public IssueBuilder WithTitle(string? title)
    {
        _title = title;
        return this;
    }

    public IssueBuilder AsSkit(string title = "[SKIT #123] Test issue")
    {
        _title = title;
        return this;
    }

    public IssueBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public IssueBuilder WithPriority(IssuePriority? priority)
    {
        _priority = priority;
        return this;
    }

    public IssueBuilder WithStatus(IssueStatus? status)
    {
        _status = status;
        return this;
    }

    public IssueBuilder WithType(IssueType? type)
    {
        _type = type;
        return this;
    }

    public IssueBuilder WithCreatedAt(DateTime? createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public IssueBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public IssueBuilder WithDeadlineAt(DateTime? deadlineAt)
    {
        _deadlineAt = deadlineAt;
        return this;
    }

    public IssueBuilder WithCompletedAt(DateTime? completedAt)
    {
        _completedAt = completedAt;
        return this;
    }

    public IssueBuilder WithCompany(Company? company)
    {
        _company = company;
        return this;
    }

    public IssueBuilder WithContact(Contact? contact)
    {
        _contact = contact;
        return this;
    }

    public IssueBuilder WithoutContact()
    {
        _contact = null;
        return this;
    }

    public IssueBuilder WithAssignee(Assignee? assignee)
    {
        _assignee = assignee;
        return this;
    }

    public IssueBuilder WithoutAssignee()
    {
        _assignee = null;
        return this;
    }

    public IssueBuilder WithComments(params Comment[] comments)
    {
        _comments = comments.ToList();
        return this;
    }

    public IssueBuilder WithoutComments()
    {
        _comments = [];
        return this;
    }

    public Issue BuildExisting()
    {
        return Issue.CreateExisting(
            id: _id,
            title: _title,
            description: _description,
            priority: _priority,
            status: _status,
            type: _type,
            createdAt: _createdAt,
            updatedAt: _updatedAt,
            deadlineAt: _deadlineAt,
            completedAt: _completedAt,
            company: _company,
            contact: _contact,
            assignee: _assignee,
            comments: _comments);
    }

    public Issue BuildNew()
    {
        return Issue.CreateNew(
            id: _id,
            title: _title,
            description: _description,
            priority: _priority,
            status: _status,
            type: _type,
            createdAt: _createdAt,
            updatedAt: _updatedAt,
            deadlineAt: _deadlineAt,
            completedAt: _completedAt,
            company: _company,
            contact: _contact,
            assignee: _assignee,
            comments: _comments);
    }
}
