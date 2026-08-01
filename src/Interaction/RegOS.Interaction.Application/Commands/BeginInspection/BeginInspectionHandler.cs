using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Inspections;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.BeginInspection;

public sealed class BeginInspectionHandler
{
    private readonly IInspectionRepository _repository;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public BeginInspectionHandler(
        IInspectionRepository repository,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<BeginInspectionResult> HandleAsync(
        BeginInspectionCommand command,
        CancellationToken cancellationToken)
    {
        var authorityExists = await _dbContext.Authorities
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.AuthorityId, cancellationToken);

        if (!authorityExists)
            throw new NotFoundException("The health authority was not found.");

        // A site the caller cannot see is indistinguishable from one that does
        // not exist — the fail-closed filter does the work (ADR-031).
        if (command.OrganizationSiteId is { } siteId)
        {
            var siteExists = await _dbContext.OrganizationSites
                .AsNoTracking()
                .AnyAsync(x => x.Id == siteId, cancellationToken);

            if (!siteExists)
                throw new NotFoundException("The site was not found.");
        }

        var inspection = Inspection.Begin(
            _tenantContext.TenantId,
            command.AuthorityId,
            command.Title,
            command.InitialStatus,
            command.OccurredOn,
            command.OrganizationSiteId,
            command.ScheduledFor);

        await _repository.AddAsync(inspection, cancellationToken);

        return new BeginInspectionResult(inspection.Id);
    }
}
