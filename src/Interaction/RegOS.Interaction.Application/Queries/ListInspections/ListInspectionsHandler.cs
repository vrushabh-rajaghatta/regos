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
            // BUG-001. The history is an OWNED collection — always loaded, and
            // no Include applies to it — so its order is settled here, in SQL,
            // where an entry id translates. Projected alongside the entity
            // rather than sorted after materialisation, where the id has no
            // IComparable and threw on the second entry.
            .Select(x => new
            {
                x.Inspection,
                x.Authority,
                // Deterministic: an entry id is unique, so this is a
                // total order.
                History = x.Inspection.History.OrderBy(h => h.Id).ToList()
            })
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
                // Deterministic: ordered by entry id in SQL above, and this
                // sort is stable (BUG-001).
                x.History
                    .OrderBy(h => h.OccurredOn)
                    .ThenBy(h => h.RecordedOnUtc)
                    .Select(h => new InspectionHistoryEntry(
                        h.Status.ToString(),
                        h.OccurredOn,
                        h.RecordedOnUtc,
                        h.Note))
                    .ToList()))
            .ToList();
    }
}
