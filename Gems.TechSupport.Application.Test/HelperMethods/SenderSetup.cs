using Gems.TechSupport.Application.Commands.Assignees;
using Gems.TechSupport.Application.Commands.Companies;
using Gems.TechSupport.Application.Commands.Contacts;
using Gems.TechSupport.Application.Commands.Issues.AddIssues;
using Gems.TechSupport.Application.Commands.Issues.UpdateIssues;
using Gems.TechSupport.Application.Commands.Okdesk;
using MediatR;
using Moq;

namespace Gems.TechSupport.Application.Test.HelperMethods
{
    public static class SenderSetup
    {
        public static void SetupOkdeskSkitIssuesCommand(Mock<ISender> sender)
        {
            sender.Setup(s => s.Send(It.IsAny<OkdeskSkitIssuesCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
        public static void SetupAddIssuesCommand(Mock<ISender> sender)
        {
            sender.Setup(s => s.Send(It.IsAny<AddIssuesCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));
        }

        public static void SetupUpdateIssuesCommand(Mock<ISender> sender)
        {
            sender.Setup(s => s.Send(It.IsAny<UpdateIssuesCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));
        }

        public static void SetupAddAssigneesCommand(Mock<ISender> sender)
        {
            sender.Setup(s => s.Send(It.IsAny<AddAssigneesCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));
        }

        public static void SetupAddCompaniesCommand(Mock<ISender> sender)
        {
            sender.Setup(s => s.Send(It.IsAny<AddCompaniesCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));
        }

        public static void SetupAddContactsCommand(Mock<ISender> sender)
        {
            sender.Setup(s => s.Send(It.IsAny<AddContactsCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));
        }
    }

}
