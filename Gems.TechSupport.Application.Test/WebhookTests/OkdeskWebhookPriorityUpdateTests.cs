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
    public class OkdeskWebhookPriorityUpdateTests
    {
        private Mock<IApplicationDbContext> dbContext = null!;
        private Mock<ILogger<OkdeskWebhookProcessingCommandHandler>> logger = null!;
        private Mock<IOkdeskNotificationTemplatesProvider> provider = null!;
        private Mock<ISender> sender = null!;
        private Mock<IResponseToDomainMapper> mapper = null!;
        private OkdeskWebhookProcessingCommandHandler _sut = null!;

        private CancellationToken ct;

        [SetUp]
        public void SetUp()
        {
            dbContext = new Mock<IApplicationDbContext>();
            logger = new Mock<ILogger<OkdeskWebhookProcessingCommandHandler>>();
            provider = new Mock<IOkdeskNotificationTemplatesProvider>();
            mapper = new Mock<IResponseToDomainMapper>();
            sender = new Mock<ISender>();

            ct = CancellationToken.None;

            _sut = new OkdeskWebhookProcessingCommandHandler(provider.Object, dbContext.Object, sender.Object, mapper.Object,logger.Object);
        }

        [Test]
        public async Task Handle_WhenPriorityUpdated_ShouldChangePriority()
        {
            // arrange
            var testAssignee = new Assignee { Id = 11, FullName = "parent Assignee" };
            var testContact = new Contact { Id = 10, FullName = "child Contact" };

            var incomingIssue = Issue.CreateExisting(
                id: 10,
                title: "Test issue",
                priority: IssuePriority.High,
                assignee: testAssignee,
                contact: testContact
            );
            var existingIssue = Issue.CreateExisting(
                id: 10,
                title: "Test issue",
                priority: IssuePriority.Low,
                assignee: testAssignee,
                contact: testContact
            );

            var issuesInDb = new List<Issue> { existingIssue };
            var issuesDbSetMock = issuesInDb.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            var webhookEvent = new PriorityUpdatedWebhookEvent
            {
                OldPriority = new PriorityResponse(
                    existingIssue.Priority.ToString()
                    + existingIssue.Assignee.ToString()
                    + existingIssue.Contact.ToString()
                    ),
                NewPriority = new PriorityResponse(IssuePriority.High.ToString()),
                Author = new WebhookEventAuthorInfo
                {
                    Id = 10,
                    Type = "user"
                }
            };

            // act
            await _sut.Handle(new OkdeskWebhookProcessingCommand(incomingIssue, webhookEvent), CancellationToken.None);

            var priorityEvent = existingIssue.DomainEvents
                .OfType<IssuePriorityUpdatedEvent>()
                .FirstOrDefault();

            // assert

            ClassicAssert.IsNotNull(priorityEvent);

            Assert.That(existingIssue.Priority, Is.EqualTo(IssuePriority.High));

            Assert.That(existingIssue.Priority, Is.EqualTo(priorityEvent.NewPriority));

            dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WhenPriorityNonChange_ShouldNothingChange()
        {
            // arrange
            var testAssignee = new Assignee { Id = 11, FullName = "parent Assignee" };
            var testContact = new Contact { Id = 10, FullName = "child Contact" };

            var incomingIssue = Issue.CreateExisting(
                id: 10,
                title: "Test issue",
                priority: IssuePriority.Normal,
                assignee: testAssignee,
                contact: testContact
            );
            var existingIssue = Issue.CreateExisting(
                id: 10,
                title: "Test issue",
                priority: IssuePriority.Normal,
                assignee: testAssignee,
                contact: testContact
            );

            var issuesInDb = new List<Issue> { existingIssue };
            var issuesDbSetMock = issuesInDb.BuildMockDbSet();
            dbContext.Setup(x => x.Issues).Returns(issuesDbSetMock.Object);

            var webhookEvent = new PriorityUpdatedWebhookEvent
            {
                OldPriority = new PriorityResponse(
                    existingIssue.Priority.ToString()
                    + existingIssue.Assignee.ToString()
                    + existingIssue.Contact.ToString()
                    ),
                NewPriority = new PriorityResponse(IssuePriority.Normal.ToString()),
                Author = new WebhookEventAuthorInfo
                {
                    Id = 10,
                    Type = "user"
                }
            };

            // act
            await _sut.Handle(new OkdeskWebhookProcessingCommand(incomingIssue, webhookEvent), CancellationToken.None);

            var priorityEvent = existingIssue.DomainEvents
                .OfType<IssuePriorityUpdatedEvent>()
                .FirstOrDefault();

            // assert

            ClassicAssert.IsNull(priorityEvent);

            Assert.That(existingIssue.Priority, Is.EqualTo(IssuePriority.Normal));

            dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
