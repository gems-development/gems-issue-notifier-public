using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Models;
using Moq;


namespace Gems.TechSupport.Application.Test.HelperMethods
{
    public static class OkdeskSetup
    {
        public static void SetupGetUpdatedIssues(
            Mock<IOkdeskService> okdesk,
            IAsyncEnumerable<IReadOnlyCollection<Issue>> incomingIssue)
        {
            okdesk.Setup(s => s.GetUpdatedIssuesAsync(
               It.IsAny<GetUpdatedIssuesRequest>(),
               It.IsAny<CancellationToken>()))
               .Returns(incomingIssue);
        }

        public static void SetupGetIssueDetailsById(
            Mock<IOkdeskService> okdesk,
            IReadOnlyCollection<Issue> incomingIssue)
        {
            okdesk.Setup(s =>
                s.GetIssueDetailsByIdAsync(It.IsAny<GetIssueDetailsByIdRequest>(), It.IsAny<CancellationToken>()))
                .Returns((GetIssueDetailsByIdRequest r, CancellationToken _) =>
                {
                    var issue = incomingIssue.First(i => i.Id == r.IssueId);
                    return Task.FromResult(MakeDetails(issue));
                });


        }
        private static async IAsyncEnumerable<IReadOnlyCollection<Issue>> AsAsyncEnumerable(List<Issue> issues)
        {
            yield return await Task.FromResult<IReadOnlyCollection<Issue>>(issues);
        }
        private static Issue MakeDetails(Issue incomingIssue)
        {
            return Issue.CreateExisting(
                incomingIssue.Id,
                incomingIssue.Title,
                incomingIssue.Description,
                incomingIssue.Priority,
                incomingIssue.Status,
                incomingIssue.Type,
                incomingIssue.CreatedAt,
                incomingIssue.UpdatedAt,
                incomingIssue.DeadlineAt,
                incomingIssue.CompletedAt,
                incomingIssue.Company,
                incomingIssue.Contact,
                incomingIssue.Assignee,
                incomingIssue.Comments.ToList()
            );

        }
    }
}
