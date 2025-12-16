using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Assignees;

internal record AddAssigneesCommand(IReadOnlyCollection<Assignee> Assignees) : ICommand;