using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Study.Application.Queries.ListStudies;

/// <summary>
/// The study registry: both kinds in one list, newest first.
/// </summary>
/// <remarks>
/// <b>Two queries and a merge, not a union in SQL.</b> They are different
/// tables with different key types, and the registry is tenant-sized rather
/// than unbounded — so the honest read is two indexed scans joined in memory,
/// which also keeps the tenant query filter doing its own work on each set
/// (ADR-031).
/// <para>
/// This is the shape ADR-040 §3 called <em>reads compose</em>: a genuine
/// question spanning two roots is answered by a projection, not by giving them
/// a parent.
/// </para>
/// </remarks>
public sealed class ListStudiesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListStudiesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StudySummary>> HandleAsync(
        ListStudiesQuery query,
        CancellationToken cancellationToken)
    {
        var clinical = await _dbContext.Set<ClinicalStudyAggregate>()
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.SponsorStudyIdentifier,
                x.Title,
                x.CreatedOn
            })
            .ToListAsync(cancellationToken);

        var nonClinical = await _dbContext.Set<NonClinicalStudyAggregate>()
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.SponsorStudyIdentifier,
                x.Title,
                x.CreatedOn
            })
            .ToListAsync(cancellationToken);

        return clinical
            .Select(x => new StudySummary(
                x.Id.Value,
                StudySummary.Clinical,
                x.SponsorStudyIdentifier,
                x.Title,
                x.CreatedOn))
            .Concat(nonClinical.Select(x => new StudySummary(
                x.Id.Value,
                StudySummary.NonClinical,
                x.SponsorStudyIdentifier,
                x.Title,
                x.CreatedOn)))
            // Deterministic: a sponsor study identifier is unique per tenant
            // — the unique index on (TenantId, SponsorStudyIdentifier).
            .OrderByDescending(x => x.CreatedOn)
            .ThenBy(x => x.SponsorStudyIdentifier, StringComparer.Ordinal)
            .ToList();
    }
}
