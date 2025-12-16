using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Issues.AddIssues;

internal record AddIssuesCommand(IReadOnlyCollection<Issue> Issues) : ICommand;