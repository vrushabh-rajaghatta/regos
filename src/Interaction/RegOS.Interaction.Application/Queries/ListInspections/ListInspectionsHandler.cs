using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Inspections;
using RegOS.Persistence;

namespace RegOS.Interaction.Application.Queries.ListInspections;

public sealed class ListInspectionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListInspectionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InspectionSummary>> HandleAsync(
        ListInspectionsQuery query,
        CancellationToken cancellationToken)
    {
        var inspections = _dbContext.Inspections.AsNoTracking();

        if (!query.IncludeConcluded)
        {
            inspections = inspections.Where(x =>
                x.CurrentStatus == InspectionStatus.Announced
                || x.CurrentStatus == InspectionStatus.InProgress);
        }

        // The site's name comes from the Organization context — a read
        // composing across a boundary, which grants no write ownership
        // (ADR-039 principle 7).
        var sites = await _dbContext.OrganizationSites
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var rows = await inspections
            .OrderBy(x => x.ScheduledFor == null)
            .ThenBy(x => x.ScheduledFor)
            .ThenBy(x => x.Id)
            .Join(
                _dbContext.Authorities.AsNoTracking(),
                x => x.AuthorityId,
                a => a.Id,
                (x, a) => new { Inspection = x, Authority = a })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new InspectionSummary(
                x.Inspection.Id.Value,
                x.Inspection.Title,
                x.Authority.Id.Value,
                x.Authority.Name,
                x.Inspection.OrganizationSiteId?.Value,
                x.Inspection.OrganizationSiteId is { } site
                    ? sites.GetValueOrDefault(site)
                    : null,
                x.Inspection.RaisedOn,
                x.Inspection.ScheduledFor,
                x.Inspection.CompletedOn,
                x.Inspection.CurrentStatus.ToString(),
                x.Inspection.Outcome,
                x.Inspection.History
                    .OrderBy(h => h.OccurredOn)
                    .ThenBy(h => h.RecordedOnUtc)
                    .ThenBy(h => h.Id)
                    .Select(h => new InspectionHistoryEntry(
                        h.Status.ToString(),
                        h.OccurredOn,
                        h.RecordedOnUtc,
                        h.Note))
                    .ToList()))
            .ToList();
    }
}
