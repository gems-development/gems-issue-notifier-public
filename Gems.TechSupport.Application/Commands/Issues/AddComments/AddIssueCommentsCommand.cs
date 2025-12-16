using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Issues.AddComments;

internal record AddIssueCommentsCommand(Issue Issue) : ICommand;
