using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;

using ApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.RegulatoryApplication.Application.Queries.Applications.ListApplicationStudies;

/// <summary>
/// The studies an application cites, newest citation first.
/// </summary>
/// <remarks>
/// Reads the DbContext directly rather than loading the aggregate (ADR-016),
/// and composes across the Study context the way every other cross-context read
/// in RegOS does — by joining ids, never by navigating.
/// </remarks>
public sealed class ListApplicationStudiesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListApplicationStudiesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CitedStudy>> HandleAsync(
        ListApplicationStudiesQuery query,
        CancellationToken cancellationToken)
    {
        var application = await _dbContext.RegulatoryApplications
            .AsNoTracking()
            .Include(x => x.StudyCitations)
            .FirstOrDefaultAsync(
                x => x.Id == query.ApplicationId, cancellationToken);

        if (application is null)
            throw new NotFoundException(
                RegulatoryApplicationErrors.ApplicationDoesNotExist);

        var citations = application.StudyCitations.ToList();

        if (citations.Count == 0) return [];

        var clinicalIds = citations
            .Select(c => c.ClinicalStudyId)
            .OfType<Study.Domain.Aggregates.ClinicalStudy.ClinicalStudyId>()
            .ToList();

        var nonClinicalIds = citations
            .Select(c => c.NonClinicalStudyId)
            .OfType<Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudyId>()
            .ToList();

        // Contains over the typed ids, not their guids: a strongly typed id's
        // converter has no SQL translation for .Value.
        var clinical = clinicalIds.Count == 0
            ? []
            : await _dbContext.Set<ClinicalStudyAggregate>()
                .AsNoTracking()
                .Where(s => clinicalIds.Contains(s.Id))
                .Select(s => new { s.Id, s.SponsorStudyIdentifier, s.Title })
                .ToListAsync(cancellationToken);

        var nonClinical = nonClinicalIds.Count == 0
            ? []
            : await _dbContext.Set<NonClinicalStudyAggregate>()
                .AsNoTracking()
                .Where(s => nonClinicalIds.Contains(s.Id))
                .Select(s => new { s.Id, s.SponsorStudyIdentifier, s.Title })
                .ToListAsync(cancellationToken);

        var byId = clinical
            .Select(s => (
                Id: s.Id.Value,
                Kind: "Clinical",
                s.SponsorStudyIdentifier,
                s.Title))
            .Concat(nonClinical.Select(s => (
                Id: s.Id.Value,
                Kind: "NonClinical",
                s.SponsorStudyIdentifier,
                s.Title)))
            .ToDictionary(s => s.Id);

        return citations
            .Where(c => byId.ContainsKey(c.StudyId))
            .Select(c =>
            {
                var study = byId[c.StudyId];

                return new CitedStudy(
                    study.Id,
                    study.Kind,
                    study.SponsorStudyIdentifier,
                    study.Title,
                    c.CitedOn);
            })
            .OrderByDescending(c => c.CitedOn)
            .ThenBy(c => c.SponsorStudyIdentifier, StringComparer.Ordinal)
            .ToList();
    }
}
