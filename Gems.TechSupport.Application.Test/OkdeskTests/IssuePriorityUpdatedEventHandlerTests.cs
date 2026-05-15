using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Application.EventHandlers;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Moq;
using NUnit.Framework;

namespace Gems.TechSupport.Application.Test.OkdeskTests
{
    [TestFixture]
    internal class IssuePriorityUpdatedEventHandlerTests
    {
        private IssuePriorityUpdatedEventHandler _sut = null!;
        private Mock<ITelegramService> telegramService = null!;
        private CancellationToken _ct;

        [SetUp]
        public void SetUp()
        {
            telegramService = new Mock<ITelegramService>();
            _ct = new CancellationToken();

            _sut = new IssuePriorityUpdatedEventHandler(telegramService.Object);
        }
        [Test]
        public async Task Handle_WhenOldAndNewPriority_ShouldSendIssueNotification()
        {
            var priorityUpdatedEventEvent = new IssuePriorityUpdatedEvent(1, 10, "User", IssuePriority.Low, IssuePriority.Normal, "contact");

            await _sut.Handle(priorityUpdatedEventEvent, CancellationToken.None);

            telegramService.Verify(c => c.SendIssuePriorityUpdatedNotificationAsync(
                priorityUpdatedEventEvent.IssueId,
                priorityUpdatedEventEvent.AssigneeId,
                priorityUpdatedEventEvent.ContactFullName,
                priorityUpdatedEventEvent.NewPriority,
                _ct
            ));
        }
    }
}