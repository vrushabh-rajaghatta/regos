using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Services;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationDivision;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

using DivisionAggregate = RegOS.Organization.Domain.Aggregates.OrganizationDivision.OrganizationDivision;

namespace RegOS.Organization.Application.Commands.CreateOrganizationDivision;

public sealed class CreateOrganizationDivisionHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly IOrganizationDivisionRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateOrganizationDivisionHandler(
        RegOSDbContext dbContext,
        IOrganizationDivisionRepository repository,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<OrganizationDivisionId> HandleAsync(
        CreateOrganizationDivisionCommand command,
        CancellationToken cancellationToken)
    {
        // One rule, so no policy service: the third parallel creation policy
        // already tested the Rule of Three, and a single existence check does
        // not earn a class of its own.
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .Where(x => x.Id == command.OrganizationId)
            .Select(x => new { x.Status })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                OrganizationSiteRuleErrors.OrganizationDoesNotExist);

        if (organization.Status != OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                OrganizationSiteRuleErrors.OrganizationInactive);

        var division = DivisionAggregate.Create(
            _tenantContext.TenantId,
            command.OrganizationId,
            command.Name,
            command.StatusDate,
            command.Acronym);

        await _repository.AddAsync(division, cancellationToken);

        return division.Id;
    }
}
