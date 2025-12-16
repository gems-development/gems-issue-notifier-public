using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Issues.UpdateIssues;

internal record UpdateIssuesCommand(IReadOnlyCollection<Issue> UpdatedIssues) : ICommand;