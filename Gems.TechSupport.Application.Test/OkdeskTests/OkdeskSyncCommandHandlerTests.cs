using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using static Gems.TechSupport.Application.Commands.Okdesk.Constants;
using Gems.TechSupport.Application.Commands.Issues.AddIssues;
using Gems.TechSupport.Application.Commands.Issues.UpdateIssues;
using Gems.TechSupport.Application.Commands.Okdesk;
using Gems.TechSupport.Application.Test.HelperMethods;
using Gems.TechSupport.Domain.Models;
using MediatR;
using Microsoft.FeatureManagement;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace Gems.TechSupport.Application.Test.OkdeskTests
{

    [TestFixture]
    class OkdeskSyncCommandHandlerTests
    {
        private OkdeskSyncCommandHandler _sut = null!;
        private Mock<IOkdeskService> okdesk = null!;
        private Mock<IApplicationDbContext> dbContext = null!;
        private Mock<ISender> sender = null!;
        private Mock<IFeatureManager> featureManager = null!;

        private CancellationToken _ct;

        [SetUp]
        public void SetUp()
        {
            okdesk = new Mock<IOkdeskService>();
            dbContext = new Mock<IApplicationDbContext>();
            sender = new Mock<ISender>();
            featureManager = new Mock<IFeatureManager>();
            _ct = CancellationToken.None;

            _sut = new OkdeskSyncCommandHandler(
                okdesk.Object,
                dbContext.Object,
                sender.Object,
                featureManager.Object);
        }
        [Test]
        public async Task Handle_WhenExistingNonSkitIssue_ShouldSendAddUpdateIssues()
        {
            // arrange
            var incomingIssues = new List<IReadOnlyCollection<Issue>>
            {
                new List<Issue>
                {
                    Issue.CreateExisting(id: 1),
                    Issue.CreateExisting(id: 2),
                    Issue.CreateExisting(id: 3),
                    Issue.CreateExisting(id: 4)

                }
            };
            IAsyncEnumerable<IReadOnlyCollection<Issue>> batch = incomingIssues.ToAsyncEnumerable();
            long[] expectedIdsForUpd = [1, 3];
            long[] expectedIdsForAdd = [2, 4];

            var issueInBd = new List<Issue> {
                Issue.CreateExisting(id:1),
                Issue.CreateExisting(id:3),
            };
            var issuesDbSetMock = issueInBd.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            OkdeskSetup.SetupGetUpdatedIssues(okdesk, batch);

            FeatureManagerSetup.SetupIsEnabledAsync(featureManager, false);

            SenderSetup.SetupOkdeskSkitIssuesCommand(sender);

            SenderSetup.SetupAddIssuesCommand(sender);
            SenderSetup.SetupUpdateIssuesCommand(sender);

            // act
            await _sut.Handle(new OkdeskSyncCommand(
                UpdatedSince: default,
                UpdatedUntil: default,
                PageSize: 100), _ct);

            // assert
            sender.Verify(s => s.Send(
                It.IsAny<OkdeskSkitIssuesCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

            sender.Verify(s => s.Send(
                It.Is<AddIssuesCommand>(c => c.Issues
                    .Select(i => i.Id)
                    .OrderBy(x => x)
                    .SequenceEqual(expectedIdsForAdd)),
                It.IsAny<CancellationToken>()),
            Times.Once);

            sender.Verify(s => s.Send(
                It.Is<UpdateIssuesCommand>(c => c.UpdatedIssues
                    .Select(i => i.Id)
                    .OrderBy(x => x)
                    .SequenceEqual(expectedIdsForUpd)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        }
        [Test]
        public async Task Handle_WhenExistingSkitIssues_FeatureEnabled_ShouldSendOnlySkitCommand()
        {
            // arrange
            var incomingIssues = new List<IReadOnlyCollection<Issue>>
            {
                new List<Issue>
                {
                    Issue.CreateExisting(id: 1, title: "[SKIT #1]"+ SkitMessagePatterns.TitleSkitResponse),
                    Issue.CreateExisting(id: 2, title: "[SKIT #1]"+ SkitMessagePatterns.TitleParent),
                    Issue.CreateExisting(id: 3, title: "[SKIT #1]"+ SkitMessagePatterns.TitleSkitResponse),
                    Issue.CreateExisting(id: 4, title: "[SKIT #1]"+ SkitMessagePatterns.TitleParent),
                }
            };
            IAsyncEnumerable<IReadOnlyCollection<Issue>> batch = incomingIssues.ToAsyncEnumerable();
            long[] expectedSkitIds = [1, 2, 3, 4];
            long[] expectedIdsForAddAndUpdate = [];

            var issueInBd = new List<Issue>
            {
                Issue.CreateExisting(id: 2, title: "[SKIT #1]"+ SkitMessagePatterns.TitleParent),
                Issue.CreateExisting(id: 4, title: "[SKIT #1]"+ SkitMessagePatterns.TitleParent),
            };
            var issuesDbSetMock = issueInBd.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            OkdeskSetup.SetupGetUpdatedIssues(okdesk, batch);

            FeatureManagerSetup.SetupIsEnabledAsync(featureManager, true);

            SenderSetup.SetupOkdeskSkitIssuesCommand(sender);

            SenderSetup.SetupAddIssuesCommand(sender);
            SenderSetup.SetupUpdateIssuesCommand(sender);

            // act
            await _sut.Handle(new OkdeskSyncCommand(
                UpdatedSince: default,
                UpdatedUntil: default,
                PageSize: 100
            ), _ct);

            // assert 
            sender.Verify(s => s.Send(
                It.Is<OkdeskSkitIssuesCommand>(c => c.Issues
                    .Select(i => i.Id)
                    .OrderBy(x => x)
                    .SequenceEqual(expectedSkitIds)),
                It.IsAny<CancellationToken>()),
            Times.Once);

            sender.Verify(s => s.Send(
                It.Is<AddIssuesCommand>(c => !c.Issues.Any()),
                It.IsAny<CancellationToken>()),
            Times.Once);

            sender.Verify(s => s.Send(
                It.Is<UpdateIssuesCommand>(c => !c.UpdatedIssues.Any()),
                It.IsAny<CancellationToken>()),
            Times.Once);
        }
        [Test]
        public async Task Handle_WhenMixedExistingIssues_ShouldSendCorrectCommands()
        {
            // arrange
            var incomingIssues = new List<IReadOnlyCollection<Issue>>
            {
                new List<Issue>
                {
                    Issue.CreateExisting(id: 1, title: "[SKIT #1]" + SkitMessagePatterns.TitleParent),
                    Issue.CreateExisting(id: 2, title: "[SKIT #1]" + SkitMessagePatterns.TitleSkitResponse),
                    Issue.CreateExisting(id: 3, title: "[SKIT #1]" + SkitMessagePatterns.TitleSlaReminder),
                    Issue.CreateExisting(id: 4, title: "[SKIT #1]" + SkitMessagePatterns.TitleNewComment, description: "Комментарий пользователя"),

                    Issue.CreateExisting(id: 5, title: "Обычная заявка 5"),
                    Issue.CreateExisting(id: 6, title: "Обычная заявка 6")
                }
            };
            IAsyncEnumerable<IReadOnlyCollection<Issue>> batch = incomingIssues.ToAsyncEnumerable();

            long[] expectedSkitIds = { 1, 2, 3, 4 };
            long[] expectedIdsForAdd = { 5 };
            long[] expectedIdsForUpd = { 6 };

            var issueInBd = new List<Issue> {
                Issue.CreateExisting(id: 6, title: "Обычная заявка 6")
            };
            var issuesDbSetMock = issueInBd.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            OkdeskSetup.SetupGetUpdatedIssues(okdesk, batch);

            FeatureManagerSetup.SetupIsEnabledAsync(featureManager, true);

            SenderSetup.SetupAddIssuesCommand(sender);
            SenderSetup.SetupUpdateIssuesCommand(sender);

            SenderSetup.SetupOkdeskSkitIssuesCommand(sender);

            // act
            await _sut.Handle(new OkdeskSyncCommand(
                UpdatedSince: default,
                UpdatedUntil: default,
                PageSize: 100
            ), _ct);

            // assert
            sender.Verify(s => s.Send(
                It.Is<OkdeskSkitIssuesCommand>(c => c.Issues
                    .Select(i => i.Id)
                    .OrderBy(x => x)
                    .SequenceEqual(expectedSkitIds)),
                It.IsAny<CancellationToken>()),
            Times.Once);

            sender.Verify(s => s.Send(
                It.Is<AddIssuesCommand>(c => c.Issues
                    .Select(i => i.Id)
                    .SequenceEqual(expectedIdsForAdd)),
                It.IsAny<CancellationToken>()),
            Times.Once);

            sender.Verify(s => s.Send(
                It.Is<UpdateIssuesCommand>(c => c.UpdatedIssues
                    .Select(i => i.Id)
                    .SequenceEqual(expectedIdsForUpd)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        }
        [Test]
        public async Task Handle_EmptyincomingIssues_SendEmptyCommands()
        {
            // arrange
            var incomingIssues = new List<IReadOnlyCollection<Issue>>{
                new List<Issue>
                {
                }
            };
            IAsyncEnumerable<IReadOnlyCollection<Issue>> batch = incomingIssues.ToAsyncEnumerable();

            var issueInBd = new List<Issue>();
            var issuesDbSetMock = issueInBd.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            OkdeskSetup.SetupGetUpdatedIssues(okdesk, batch);

            FeatureManagerSetup.SetupIsEnabledAsync(featureManager, false);

            SenderSetup.SetupAddIssuesCommand(sender);
            SenderSetup.SetupUpdateIssuesCommand(sender);

            SenderSetup.SetupOkdeskSkitIssuesCommand(sender);

            // act
            await _sut.Handle(new OkdeskSyncCommand(
                UpdatedSince: default,
                UpdatedUntil: default,
                PageSize: 100
            ), _ct);

            // assert 
            sender.Verify(s => s.Send(
                    It.IsAny<OkdeskSkitIssuesCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            sender.Verify(s => s.Send(
                    It.Is<AddIssuesCommand>(c => !c.Issues.Any()),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            sender.Verify(s => s.Send(
                    It.Is<UpdateIssuesCommand>(c => !c.UpdatedIssues.Any()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

    }
}