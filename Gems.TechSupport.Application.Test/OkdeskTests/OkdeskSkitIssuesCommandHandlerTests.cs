using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Commands.Issues.AddIssues;
using Gems.TechSupport.Application.Commands.Issues.UpdateIssues;
using Gems.TechSupport.Application.Commands.Okdesk;
using static Gems.TechSupport.Application.Commands.Okdesk.Constants;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Application.Test.HelperMethods;
using Gems.TechSupport.Domain.Models;
using MediatR;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace Gems.TechSupport.Application.Test.OkdeskTests
{
    [TestFixture]
    public class OkdeskSkitIssuesCommandHandlerTests
    {
        private Mock<IApplicationDbContext> dbContext = null!;
        private Mock<IOkdeskService> okdesk = null!;
        private Mock<ISender> sender = null!;
        private OkdeskSkitIssuesCommandHandler _sut = null!;
        private CancellationToken ct;

        [SetUp]
        public void SetUp()
        {
            dbContext = new Mock<IApplicationDbContext>();
            okdesk = new Mock<IOkdeskService>();
            sender = new Mock<ISender>();
            ct = CancellationToken.None;

            _sut = new OkdeskSkitIssuesCommandHandler(
                dbContext.Object,
                okdesk.Object,
                sender.Object);
        }
        [Test]
        public async Task Handle_WhenNewParentSkitIssue_ShouldSeparatedIntoAddAndUpdate()
        {
            // arrange
            var parentAssignee = new Assignee
            {
                Id = 11,
                FullName = "parent Assignee"
            };
            var incomingParentIssue = Issue.CreateNew(
                id: 3,
                title: "[SKIT #123] " + SkitMessagePatterns.TitleParent,
                assignee: parentAssignee
            );
            var incomingIssues = new List<Issue> { incomingParentIssue };

            var issuesInDb = new List<Issue>();
            var issuesDbSetMock = issuesInDb.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            SenderSetup.SetupAddIssuesCommand(sender);
            SenderSetup.SetupUpdateIssuesCommand(sender);

            // act
            await _sut.Handle(new OkdeskSkitIssuesCommand(incomingIssues), ct);

            // assert
            sender.Verify(s => s.Send(
                It.Is<AddIssuesCommand>(c => c.Issues
                    .Select(i => i.Id)
                    .SequenceEqual(new long[] { 3 })),
                It.IsAny<CancellationToken>()),
            Times.Once);

            sender.Verify(s => s.Send(
                It.Is<UpdateIssuesCommand>(c => !c.UpdatedIssues.Any()),
                It.IsAny<CancellationToken>()),
            Times.Once);

            okdesk.Verify(x => x.DeleteIssueAsync(
                It.IsAny<DeleteIssueRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        }
        [Test]
        public async Task Handle_WhenExistingIssueNewCommentAndAuthorSupport_ShouldDeleteWithoutMerge()
        {
            // arrange
            var incomingIssues = new List<Issue>
            {
                Issue.CreateNew(
                    2,
                    "[SKIT #123] " + SkitMessagePatterns.TitleNewComment,
                    description: SkitMessagePatterns.AuthorSupport
                )
            };

            var issuesInDb = new List<Issue>();
            var issuesDbSetMock = issuesInDb.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            OkdeskSetup.SetupGetIssueDetailsById(okdesk, incomingIssues);

            // act
            await _sut.Handle(new OkdeskSkitIssuesCommand(incomingIssues), ct);

            // assert
            okdesk.Verify(x => x.DeleteIssueAsync(
                It.Is<DeleteIssueRequest>(r => r.IssueId == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);

            okdesk.Verify(x => x.PostCommentAsync(
                It.IsAny<PostIssueCommentRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        }
        [Test]
        public async Task Handle_WhenMixedExistingParentAndChildSkitIssue_ShouldMergeChildAndDelete()
        {
            // arrange
            string childDesc = SkitMessagePatterns.AuthorSupport;

            var parentAssignee = new Assignee { Id = 11, FullName = "parent Assignee" };

            var childAssignee = new Assignee { Id = 10, FullName = "child Assignee" };
            var childContact = new Contact { Id = 10, FullName = "child Contact" };

            var incomingIssues = new List<Issue> {
                Issue.CreateNew(
                    id: 4,
                    title: "[SKIT #123] " + SkitMessagePatterns.TitleSlaReminder,
                    description: childDesc,
                    contact: childContact,
                    assignee: childAssignee
            )};
            var issuesInDb = new List<Issue> {
                Issue.CreateExisting(
                    3,
                    "[SKIT #123] " + SkitMessagePatterns.TitleParent,
                    assignee: parentAssignee
            )};

            var issuesDbSetMock = issuesInDb.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            OkdeskSetup.SetupGetIssueDetailsById(okdesk, incomingIssues);

            // act
            await _sut.Handle(new OkdeskSkitIssuesCommand(incomingIssues), ct);

            // assert
            okdesk.Verify(x => x.PostCommentAsync(
                It.Is<PostIssueCommentRequest>(r =>
                    r.IssueId == 3 &&
                    r.Content.Contains(childDesc) &&
                    r.AuthorId == 10
                ),
                It.IsAny<CancellationToken>()),
            Times.Once);

            okdesk.Verify(x => x.DeleteIssueAsync(
                It.Is<DeleteIssueRequest>(r => r.IssueId == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
        }
    }
}
