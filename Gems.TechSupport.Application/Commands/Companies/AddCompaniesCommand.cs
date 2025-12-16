using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Companies;

internal record AddCompaniesCommand(IReadOnlyCollection<Company> Companies) : ICommand;
