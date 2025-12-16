using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Okdesk;

public record OkdeskSkitIssuesCommand(IReadOnlyCollection<Issue> Issues, int? PageSize = null) : ICommand;
