using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Shared.CQRS;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Application.Commands.Issues.AddComments;

internal sealed class AddIssueCommentsCommandHandler(IApplicationDbContext dbContext, IOkdeskService okdeskService) 
    : ICommandHandler<AddIssueCommentsCommand>
{
    public async Task Handle(AddIssueCommentsCommand request, CancellationToken cancellationToken)
    {
        var issue = request.Issue;
        var query = new GetIssueCommentsRequest(issue.Id);

        var comments = await okdeskService.GetIssueCommentsAsync(query, cancellationToken);

        foreach (var comment in comments)
        {
            var contact = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == request.Issue.Contact!.Id, cancellationToken);
            comment.Contact = contact!;

            issue.AddComment(comment);
        }
    }
}
