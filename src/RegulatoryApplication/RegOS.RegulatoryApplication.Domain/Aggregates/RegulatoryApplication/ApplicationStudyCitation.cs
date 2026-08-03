using RegOS.SharedKernel.Abstractions;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

/// <summary>
/// A study this application is supported by — the answer to *"which studies
/// support this filing?"*, and read the other way, *"which filings cite this
/// study?"*.
/// </summary>
/// <remarks>
/// <b>A child of the citing application, not a join aggregate owned by
/// neither.</b> The Phase-1 sketch leaned toward a join, on the grounds that
/// both directions are queried and neither side is the natural owner. That
/// second clause is false: **a citation is a claim the application makes.**
/// Nothing about the study changes when a filing cites it, and removing the
/// citation is the application changing its mind.
/// <para>
/// <b>And a join aggregate would be built for a third citer that does not
/// exist.</b> ADR-018 permits abstracting on the third demonstrated need;
/// `Registration → Clinical Study` and a commitment's study are both plausible
/// and neither has been asked for. Two shapes is a duplication, which is what
/// the rule of three expects to see before it acts.
/// </para>
/// <para>
/// The exclusive-or is the same shape <c>SubmissionDocument</c> carries, and is
/// deliberately duplicated rather than extracted — that is the second
/// occurrence, and ADR-018 says the third is when to look again.
/// </para>
/// </remarks>
public sealed class ApplicationStudyCitation
    : Entity<ApplicationStudyCitationId>
{
    // EF materialisation only.
    private ApplicationStudyCitation()
    {
    }

    internal ApplicationStudyCitation(
        ApplicationStudyCitationId id,
        ClinicalStudyId? clinicalStudyId,
        NonClinicalStudyId? nonClinicalStudyId,
        DateTime citedOn)
    {
        Id = id;
        ClinicalStudyId = clinicalStudyId;
        NonClinicalStudyId = nonClinicalStudyId;
        CitedOn = citedOn;
    }

    public ClinicalStudyId? ClinicalStudyId { get; private set; }

    public NonClinicalStudyId? NonClinicalStudyId { get; private set; }

    /// <summary>
    /// When the citation was recorded. RegOS's clock, not a business date —
    /// nothing regulatory turns on it, and a study's own dates belong to the
    /// study if they are ever asked for (ADR-056 §3).
    /// </summary>
    public DateTime CitedOn { get; private set; }

    /// <summary>The study, whichever kind it is, as a plain guid.</summary>
    public Guid StudyId
        => ClinicalStudyId?.Value ?? NonClinicalStudyId!.Value;

    public bool Names(ClinicalStudyId studyId)
        => ClinicalStudyId == studyId;

    public bool Names(NonClinicalStudyId studyId)
        => NonClinicalStudyId == studyId;
}
