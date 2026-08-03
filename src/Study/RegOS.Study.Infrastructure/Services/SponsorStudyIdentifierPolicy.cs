using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Study.Application;
using RegOS.Study.Application.Services;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Study.Infrastructure.Services;

/// <inheritdoc cref="ISponsorStudyIdentifierPolicy"/>
public sealed class SponsorStudyIdentifierPolicy : ISponsorStudyIdentifierPolicy
{
    private readonly RegOSDbContext _dbContext;

    public SponsorStudyIdentifierPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureUnusedAsync(
        TenantId tenantId,
        string sponsorStudyIdentifier,
        Guid? excluding,
        CancellationToken cancellationToken)
    {
        // The tenant is in the predicate as well as in the query filter
        // (ADR-031). Redundant, and deliberately so: this is the one rule whose
        // correctness a reader should be able to see without knowing the filter
        // is there.
        var clinical = await _dbContext.Set<ClinicalStudyAggregate>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.SponsorStudyIdentifier == sponsorStudyIdentifier)
            .Select(x => new { x.Id, x.Title })
            .FirstOrDefaultAsync(cancellationToken);

        // Unwrapped here rather than in the projection: a strongly typed id's
        // converter has no SQL translation for .Value. The two sets also
        // project to different anonymous types — their ids are different
        // classes — so one shape is chosen for both.
        (Guid Id, string Title)? holder = clinical is null
            ? null
            : (clinical.Id.Value, clinical.Title);

        if (holder is null || holder.Value.Id == excluding)
        {
            var nonClinical = await _dbContext.Set<NonClinicalStudyAggregate>()
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId
                    && x.SponsorStudyIdentifier == sponsorStudyIdentifier)
                .Select(x => new { x.Id, x.Title })
                .FirstOrDefaultAsync(cancellationToken);

            holder = nonClinical is null
                ? null
                : (nonClinical.Id.Value, nonClinical.Title);
        }

        if (holder is null || holder.Value.Id == excluding)
            return;

        throw new BusinessRuleViolationException(
            StudyRuleErrors.SponsorStudyIdentifierAlreadyUsed(
                sponsorStudyIdentifier, holder.Value.Title));
    }
}
