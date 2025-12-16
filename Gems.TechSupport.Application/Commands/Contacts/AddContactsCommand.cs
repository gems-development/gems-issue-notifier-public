using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Contacts;

internal record AddContactsCommand(IReadOnlyCollection<Contact> Contacts) : ICommand;
