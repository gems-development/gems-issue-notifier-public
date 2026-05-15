using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Abstractions.Responses;
using Gems.TechSupport.Application.Commands.Okdesk;
using Gems.TechSupport.Application.Responses.Models;
using Gems.TechSupport.Application.Responses.Webhooks;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gems.TechSupport.Application.Test.WebhookTests
{
    [TestFixture]
    internal class OkdeskWebhookStatusUpdateTests
    {
        private Mock<IApplicationDbContext> dbContext = null!;
        private Mock<ILogger<OkdeskWebhookProcessingCommandHandler>> logger = null!;
        private Mock<IOkdeskNotificationTemplatesProvider> provider = null!;
        private Mock<ISender> sender = null!;
        private Mock<IResponseToDomainMapper> mapper = null!;
        private OkdeskWebhookProcessingCommandHandler _sut = null!;

        [SetUp]
        public void SetUp()
        {
            dbContext = new Mock<IApplicationDbContext>();
            logger = new Mock<ILogger<OkdeskWebhookProcessingCommandHandler>>();
            provider = new Mock<IOkdeskNotificationTemplatesProvider>();
            sender = new Mock<ISender>();
            mapper = new Mock<IResponseToDomainMapper>();

            _sut = new OkdeskWebhookProcessingCommandHandler(provider.Object, dbContext.Object,sender.Object, mapper.Object, logger.Object);
        }
        [Test]
        public async Task Handle_WhenStatusUpdateWebhook_ExistingIssue_UpdatesStatus()
        {
            //arrange
            var testAssignee = new Assignee { Id = 11, FullName = "parent Assignee" };
            var testContact = new Contact { Id = 10, FullName = "child Contact" };

            var incomingIssue = Issue.CreateExisting(
                id: 1001,
                status: IssueStatus.InWork,
                type: IssueType.Incident,
                contact: testContact,
                assignee: testAssignee,
                priority: IssuePriority.Normal);
            var existingIssue = Issue.CreateExisting(
                id: 1001,
                status: IssueStatus.Opened,
                type: IssueType.Incident,
                contact: testContact,
                assignee: testAssignee,
                priority: IssuePriority.Normal);

            var issuesInDb = new List<Issue> { existingIssue };
            var issuesDbSetMock = issuesInDb.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            var statusEvent = new StatusUpdatedWebhookEvent
            {
                Author = new WebhookEventAuthorInfo { Id = 1, Type = "employee" },
                OldStatus = new StatusResponse("opened"),
                NewStatus = new StatusResponse("inwork")
            };

            //act
            await _sut.Handle(new OkdeskWebhookProcessingCommand(incomingIssue, statusEvent), CancellationToken.None);
            var deadlineNotificationEvent = existingIssue.DomainEvents
                    .OfType<IssueDeadlineNotificationEvent>()
                    .FirstOrDefault();

            //assert
            ClassicAssert.IsNotNull(deadlineNotificationEvent);

            Assert.That(existingIssue.Status, Is.EqualTo(IssueStatus.InWork));

            Assert.That(issuesInDb[0].DomainEvents.Any(e => e is IssueDeadlineNotificationEvent), Is.True);

            dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
