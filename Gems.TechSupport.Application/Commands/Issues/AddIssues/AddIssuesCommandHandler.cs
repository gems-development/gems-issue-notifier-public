using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Commands.Issues.AddComments;
using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Domain.Shared.CQRS;
using MediatR;

namespace Gems.TechSupport.Application.Commands.Issues.AddIssues;

internal sealed class AddIssueCommandHandler(IApplicationDbContext dbContext, ISender sender) 
    : ICommandHandler<AddIssuesCommand>
{
    public async Task Handle(AddIssuesCommand request, CancellationToken cancellationToken)
    {
        var issues = request.Issues;
        await issues.LoadReferenceEntities(dbContext, cancellationToken);

        foreach (var issue in issues)
        {
            var addComments = new AddIssueCommentsCommand(issue);
            await sender.Send(addComments, cancellationToken);
        }

        dbContext.Issues.AddRange(issues);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}