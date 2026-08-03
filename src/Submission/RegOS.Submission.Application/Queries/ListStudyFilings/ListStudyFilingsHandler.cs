using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Queries.ListStudyFilings;

/// <summary>
/// Every filing that names a study — the application-level citations and the
/// sequences whose placements report it.
/// </summary>
/// <remarks>
/// <b>It lives here rather than in the Study context.</b> A study does not know
/// where it is filed (ADR-056 §4), and putting this query beside the aggregate
/// would give `Study` a dependency on both of its citers — the inversion that
/// whole decision exists to prevent.
/// <para>
/// It lives here rather than in `RegulatoryApplication` for the same reason
/// read the other way: `Submission` already depends on `RegulatoryApplication`,
/// so this is the only context that can see both without a new edge — and a
/// `RegulatoryApplication → Submission` reference would close a cycle.
/// </para>
/// <para>
/// This is ADR-039 principle 7 at its plainest: <b>a real question spanning
/// three contexts is a read, and a read grants nobody write ownership.</b>
/// </para>
/// </remarks>
public sealed class ListStudyFilingsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListStudyFilingsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StudyFiling>> HandleAsync(
        ListStudyFilingsQuery query,
        CancellationToken cancellationToken)
    {
        // Compared as typed ids, never unwrapped: a strongly typed id's
        // converter has no SQL translation for .Value, and unwrapping in a
        // predicate pushes the whole query to client evaluation — which EF
        // refuses outright rather than doing quietly.
        var clinicalId = ClinicalStudyId.From(query.StudyId);
        var nonClinicalId = NonClinicalStudyId.From(query.StudyId);

        var cited = await (
            from application in _dbContext.RegulatoryApplications
                .AsNoTracking()
            from citation in application.StudyCitations
            where citation.ClinicalStudyId == clinicalId
                || citation.NonClinicalStudyId == nonClinicalId
            select new
            {
                application.Id,
                application.Name,
                application.ApplicationNumber,
            }).ToListAsync(cancellationToken);

        var reported = await (
            from placement in _dbContext.Set<SubmissionDocument>()
                .AsNoTracking()
            where placement.ClinicalStudyId == clinicalId
                || placement.NonClinicalStudyId == nonClinicalId
            join submission in _dbContext.Set<SubmissionAggregate>()
                on EF.Property<SubmissionId>(placement, "SubmissionId")
                equals submission.Id
            join application in _dbContext.RegulatoryApplications
                on submission.ApplicationId equals application.Id
            select new
            {
                ApplicationId = application.Id,
                ApplicationName = application.Name,
                application.ApplicationNumber,
                SubmissionId = submission.Id,
                SubmissionTitle = submission.Title,
                submission.SequenceNumber,
            }).ToListAsync(cancellationToken);

        var applications = cited
            .DistinctBy(a => a.Id)
            .Select(a => new StudyFiling(
                "Application",
                a.Id.Value,
                a.Name,
                a.ApplicationNumber,
                null,
                null,
                null));

        // One row per sequence, not per placement: a sequence filing four
        // documents about one study is one filing, and four rows would read as
        // four.
        var sequences = reported
            .DistinctBy(s => s.SubmissionId)
            .Select(s => new StudyFiling(
                "Sequence",
                s.ApplicationId.Value,
                s.ApplicationName,
                s.ApplicationNumber,
                s.SubmissionId.Value,
                s.SubmissionTitle,
                s.SequenceNumber is { } number ? $"{number:0000}" : null));

        return applications
            .Concat(sequences)
            .OrderBy(f => f.ApplicationName, StringComparer.Ordinal)
            .ThenBy(f => f.SequenceNumber, StringComparer.Ordinal)
            .ToList();
    }
}
