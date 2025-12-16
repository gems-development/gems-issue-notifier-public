using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Commands.Issues.AddIssues;
using Gems.TechSupport.Application.Commands.Issues.UpdateIssues;
using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace Gems.TechSupport.Application.Commands.Okdesk;

internal sealed class OkdeskSkitIssuesCommandHandler(
    IApplicationDbContext dbContext,
    IOkdeskService okdeskService,
    ISender sender)
    : ICommandHandler<OkdeskSkitIssuesCommand>
{
    public async Task Handle(OkdeskSkitIssuesCommand request, CancellationToken cancellationToken)
    {
        var issues = request.Issues;

        var parentIssues = issues.Where(x => x.Title?.Contains(Constants.SkitMessagePatterns.TitleParent) is true).ToList();
        await SyncParentIssues(parentIssues, cancellationToken);

        var childIssues = issues.Except(parentIssues).ToList();

        foreach (var issue in childIssues)
        {
            var getIssueDetailsRequest = new GetIssueDetailsByIdRequest(issue.Id);
            var issueDetails = await okdeskService.GetIssueDetailsByIdAsync(getIssueDetailsRequest, cancellationToken);

            var title = issueDetails.Title!;
            var description = issueDetails.Description!;

            if (title.Contains(Constants.SkitMessagePatterns.TitleNewComment) && description.Contains(Constants.SkitMessagePatterns.AuthorSupport))
            {
                await DeleteIssueFromOkdesk(issueDetails, cancellationToken);
            }
            else
            {
                if (title.Contains(Constants.SkitMessagePatterns.TitleSlaReminder) ||
                    title.Contains(Constants.SkitMessagePatterns.TitleSkitResponse) ||
                    title.Contains(Constants.SkitMessagePatterns.TitleNewComment))
                {
                    await MergeIssueToComment(issueDetails, cancellationToken);
                }
            }
        }
    }

    private async Task MergeIssueToComment(Issue issue, CancellationToken cancellationToken)
    {
        var title = issue.Title!;
        var description = issue.Description!;

        var match = Regex.Match(title, Constants.SkitMessagePatterns.SkitTagPattern);
        var skitNumber = match.Groups["num"].Value;
        var parentIssue = await FindParentSkitIssue(skitNumber, cancellationToken);

        if (parentIssue is null)
        {
            return;
        }

        string commentContent = BuildMergeComment(issue.Id, title, description);

        var postCommentIssueRequest = new PostIssueCommentRequest(parentIssue.Id, commentContent, issue.Assignee!.Id);
        await okdeskService.PostCommentAsync(postCommentIssueRequest, cancellationToken);

        if (parentIssue.Status != IssueStatus.InWork)
        {
            var setIssueStatusInWorkRequest = new SetIssueStatusRequest(parentIssue.Id, IssueStatus.InWork);
            await okdeskService.SetIssueStatusAsync(setIssueStatusInWorkRequest, cancellationToken);
        }

        await DeleteIssueFromOkdesk(issue, cancellationToken);
    }

    private Task DeleteIssueFromOkdesk(Issue issue, CancellationToken cancellationToken)
    {
        var deleteIssueRequest = new DeleteIssueRequest(issue.Id);
        return okdeskService.DeleteIssueAsync(deleteIssueRequest, cancellationToken);
    }

    private async Task<Issue?> FindParentSkitIssue(string skitNumber, CancellationToken cancellationToken)
    {
        var parentTitle = string.Format(Constants.SkitMessagePatterns.ParentTitleTemplate, skitNumber);

        var parentIssue = await dbContext.Issues
            .FirstOrDefaultAsync(x => x.Title!.Contains(parentTitle), cancellationToken);

        return parentIssue;
    }

    private async Task SyncParentIssues(IReadOnlyCollection<Issue> issues, CancellationToken cancellationToken)
    {
        var (issuesToAddInDb, issuesToUpdateInDb) = await issues.SeparateForAddAndUpdate(dbContext, cancellationToken);

        var addIssuesCommand = new AddIssuesCommand(issuesToAddInDb);
        var updateIssuesCommand = new UpdateIssuesCommand(issuesToUpdateInDb);

        await sender.Send(addIssuesCommand, cancellationToken);
        await sender.Send(updateIssuesCommand, cancellationToken);
    }
    
    private string BuildMergeComment(long childIssueId, string title, string description)
    {
        var commentBuilder = new StringBuilder();

        commentBuilder
            .Append("<span class=\"in-comment-issue\">Комментарий добавлен при объединении c заявкой " +
            $"<i>№{childIssueId}</i> от <i>{DateTime.Now:dd.MM.yyyy}</i></span><br><br>");

        commentBuilder
            .Append($"<b>Тема заявки</b>:{title}<br><br>");

        commentBuilder
            .Append($"<b>Описание заявки</b>:{description}<br><br>");

        return commentBuilder.ToString();
    }
}
