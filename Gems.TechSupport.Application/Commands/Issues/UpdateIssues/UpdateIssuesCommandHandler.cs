using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Commands.Issues.AddComments;
using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Domain.Shared.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Application.Commands.Issues.UpdateIssues;

internal sealed class UpdateIssuesCommandHandler(IApplicationDbContext dbContext, ISender sender) 
    : ICommandHandler<UpdateIssuesCommand>
{
    public async Task Handle(UpdateIssuesCommand request, CancellationToken cancellationToken)
    {
        var updatedIssues = request.UpdatedIssues;
        await updatedIssues.LoadReferenceEntities(dbContext, cancellationToken);

        var updatedIssuesIds = updatedIssues.Select(x => x.Id).ToList();

        var issuesInDb = await dbContext.Issues
            .Where(x => updatedIssuesIds.Contains(x.Id))
            .Include(x => x.Comments)
            .Include(x => x.Assignee)
            .ToListAsync(cancellationToken);

        var updatedIssuesDict = updatedIssues.ToDictionary(x => x.Id);

        foreach (var issueInDb in issuesInDb)
        {
            if (updatedIssuesDict.TryGetValue(issueInDb.Id, out var updatedIssue))
            {
                var addCommentsCommand = new AddIssueCommentsCommand(updatedIssue);
                await sender.Send(addCommentsCommand, cancellationToken);

                issueInDb.Update(updatedIssue);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}