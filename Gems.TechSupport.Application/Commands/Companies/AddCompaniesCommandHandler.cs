using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Domain.Shared.CQRS;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Application.Commands.Companies;

internal sealed class AddCompaniesCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<AddCompaniesCommand>
{
    public async Task Handle(AddCompaniesCommand request, CancellationToken cancellationToken)
    {
        var companies = request.Companies;
        var companyIds = companies
            .Select(x => x.Id)
            .ToList();

        var companiesInDb = await dbContext.Companies
            .AsNoTracking()
            .Where(x => companyIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var companyIdsToAddInDb = companyIds.Except(companiesInDb).ToList();

        var companiesToAddInDb = companies.Where(x => companyIdsToAddInDb.Contains(x.Id)).ToList();

        dbContext.Companies.AddRange(companiesToAddInDb);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
