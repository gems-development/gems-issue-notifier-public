using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Models;

public class Contact : AuditableEntity
{
    private readonly List<Company> _companies = [];
    private readonly List<Comment> _comments = [];
    private readonly List<Issue> _issues = [];

    public string FullName { get; init; } = string.Empty;
    public IReadOnlyList<Company> Companies => _companies.AsReadOnly();
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyList<Issue> Issues => _issues.AsReadOnly();
}
