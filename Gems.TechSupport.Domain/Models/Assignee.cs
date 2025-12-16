using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Models;

public class Assignee : AuditableEntity
{
    private readonly List<Issue> _issues = [];

    public required string FullName { get; init; }
    public IReadOnlyList<Issue> Issues => _issues.AsReadOnly();
}
