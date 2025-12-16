using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Models;

public class Company : AuditableEntity
{
    private readonly List<Issue> _issues = [];

    public required string CompanyName { get; init; }
    public Contact? Contact { get; set; }
    public IReadOnlyList<Issue> Issues => _issues.AsReadOnly();
}
