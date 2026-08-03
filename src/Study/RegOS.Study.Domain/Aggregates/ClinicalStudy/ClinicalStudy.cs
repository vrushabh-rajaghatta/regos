using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Study.Domain.Aggregates.ClinicalStudy;

/// <summary>
/// A study in human subjects that documents in the dossier report on.
/// </summary>
/// <remarks>
/// <b>Its identity is the sponsor's, not RegOS's and not the authority's</b>
/// (ADR-056). ICH defines the study-id as <em>"the internal alphanumeric code
/// used by the sponsor to unambiguously identify this study"</em> (E29), and
/// <see cref="Id"/> is only RegOS's handle on the row.
/// <para>
/// <b>Two facts, deliberately.</b> ICH requires a study's species, route,
/// duration and type-of-control only for CTD 4.2.3.1, 4.2.3.2, 4.2.3.4.1 and
/// 5.3.5.1, and the seeded FDA IND blueprint offers none of those — so nothing
/// in RegOS today can demand more than an identifier and a title. Phase, arms,
/// indication, subject count and the rest of RIM's list arrive with a workflow
/// that needs them, per ADR-056 §3, and never because a reference model lists
/// them.
/// </para>
/// <para>
/// <b>Separate from <c>NonClinicalStudy</c> because the domain differs, not
/// because today's properties do.</b> For this story the two carry the same two
/// facts; they are still two aggregates, and neither gets a shared base class or
/// a kind discriminator to hold the duplication (ADR-056 §2, ADR-040 §3).
/// </para>
/// <para>
/// <b>No status, because nothing removes a study.</b> ES-018's Active/Inactive
/// pair exists so records are retired rather than deleted; this story offers no
/// deletion, so a lifecycle would be a column no capability writes — the
/// "persistent property with no acquisition path" EPIC-007a spent three findings
/// on.
/// </para>
/// </remarks>
public sealed class ClinicalStudy : AggregateRoot<ClinicalStudyId>
{
    /// <summary>
    /// Generous for a sponsor code (they run to about a dozen characters), and
    /// bounded because S003 puts this in a filename: an STF is written as
    /// <c>stf-&lt;study-id&gt;.xml</c>, and FDA caps a whole path at 150
    /// characters (E22).
    /// </summary>
    public const int SponsorStudyIdentifierMaxLength = 50;

    public const int TitleMaxLength = 500;

    private ClinicalStudy()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    /// <summary>
    /// What the sponsor calls this study. Screen word: <b>Study ID</b>.
    /// </summary>
    /// <remarks>
    /// Not named <c>StudyId</c>: that would read as this aggregate's identity,
    /// which it is not. <b>It must be identical wherever it appears across
    /// sequences</b> — FDA's review tooling recognises one study by it, and a
    /// mismatch shows the reviewer two (E24). That constraint is the reason the
    /// study is owned here rather than by a submission.
    /// </remarks>
    public string SponsorStudyIdentifier { get; private set; } = default!;

    /// <summary>
    /// The full title of the study — <em>"not the title of each individual
    /// document"</em> (E29).
    /// </summary>
    public string Title { get; private set; } = default!;

    public DateTime CreatedOn { get; private set; }

    public static ClinicalStudy Register(
        TenantId tenantId,
        string sponsorStudyIdentifier,
        string title)
    {
        if (tenantId is null)
            throw new DomainException(ClinicalStudyErrors.TenantRequired);

        var study = new ClinicalStudy
        {
            TenantId = tenantId,
            SponsorStudyIdentifier =
                ValidatedIdentifier(sponsorStudyIdentifier),
            Title = ValidatedTitle(title),
            CreatedOn = DateTime.UtcNow
        };

        study.Id = ClinicalStudyId.New();

        return study;
    }

    /// <summary>
    /// Corrects the title. A study registered before its protocol was final is
    /// the ordinary case, and a typo that can never be fixed is the debt this
    /// project has already paid once.
    /// </summary>
    /// <remarks>
    /// <b>Unguarded today, and that becomes wrong in S002.</b> E24 makes the
    /// title part of what FDA matches on, so renaming a study already named in a
    /// published sequence would split it in two in the reviewer's tool. Nothing
    /// can cite a study yet, so there is no such sequence to protect; the moment
    /// a placement can name one, this needs the policy shape
    /// <c>ApplicationNumberPolicy</c> uses — refuse a change once something
    /// citing it has been published.
    /// </remarks>
    public void Retitle(string title)
    {
        Title = ValidatedTitle(title);
    }

    /// <remarks>
    /// <b>No format rule.</b> ICH says alphanumeric, but the domain does not
    /// police it: EPIC-007a settled that an authority's format check belongs at
    /// the boundary that needs it — <c>RecordApplicationNumber</c> takes any
    /// string and the generator refuses a non-FDA one by name. S003 does the
    /// same for an identifier it cannot put in a filename.
    /// </remarks>
    private static string ValidatedIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(
                ClinicalStudyErrors.SponsorStudyIdentifierRequired);

        var trimmed = value.Trim();

        if (trimmed.Length > SponsorStudyIdentifierMaxLength)
            throw new DomainException(
                ClinicalStudyErrors.SponsorStudyIdentifierTooLong);

        return trimmed;
    }

    private static string ValidatedTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(ClinicalStudyErrors.TitleRequired);

        var trimmed = value.Trim();

        if (trimmed.Length > TitleMaxLength)
            throw new DomainException(ClinicalStudyErrors.TitleTooLong);

        return trimmed;
    }
}
