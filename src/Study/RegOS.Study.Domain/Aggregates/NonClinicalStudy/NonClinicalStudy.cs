using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Study.Domain.Aggregates.NonClinicalStudy;

/// <summary>
/// A study not in human subjects — toxicology, pharmacology, pharmacokinetics —
/// that documents in CTD Module 4 report on.
/// </summary>
/// <remarks>
/// <b>This is the one that blocks an IND.</b> Every IND carries Module 4
/// content, FDA requires a Study Tagging File for every file in 4.2.x (E21), and
/// an STF cannot be written without a study to tag against — which is why
/// <c>SequenceFolderGenerator</c> refuses such a placement by name today
/// (ADR-054 §6).
/// <para>
/// <b>A peer of <c>ClinicalStudy</c>, not a variant of it</b> (ADR-056 §2). They
/// carry the same two facts in this story and remain separate aggregates: the
/// STF's <c>category</c> vocabulary is kind-specific — species, route, duration
/// and type-of-control apply to 4.2.3.1, 4.2.3.2 and 4.2.3.4.1 on this side and
/// 5.3.5.1 on the other — so the divergence is in the domain rather than in
/// today's column list.
/// </para>
/// <para>
/// See <see cref="ClinicalStudy"/> for why there are two facts, no status, and
/// no format rule on the identifier. The reasoning is identical and is not
/// abstracted, per ADR-018.
/// </para>
/// </remarks>
public sealed class NonClinicalStudy : AggregateRoot<NonClinicalStudyId>
{
    /// <summary>
    /// Bounded because S003 puts this in a filename — <c>stf-&lt;study-id&gt;.xml</c>
    /// — and FDA caps a whole path at 150 characters (E22).
    /// </summary>
    public const int SponsorStudyIdentifierMaxLength = 50;

    public const int TitleMaxLength = 500;

    private NonClinicalStudy()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    /// <summary>
    /// What the sponsor calls this study. Screen word: <b>Study ID</b>. Must be
    /// identical wherever it appears across sequences (E24).
    /// </summary>
    public string SponsorStudyIdentifier { get; private set; } = default!;

    /// <summary>
    /// The full title of the study — <em>"not the title of each individual
    /// document"</em> (E29).
    /// </summary>
    public string Title { get; private set; } = default!;

    public DateTime CreatedOn { get; private set; }

    public static NonClinicalStudy Register(
        TenantId tenantId,
        string sponsorStudyIdentifier,
        string title)
    {
        if (tenantId is null)
            throw new DomainException(NonClinicalStudyErrors.TenantRequired);

        var study = new NonClinicalStudy
        {
            TenantId = tenantId,
            SponsorStudyIdentifier =
                ValidatedIdentifier(sponsorStudyIdentifier),
            Title = ValidatedTitle(title),
            CreatedOn = DateTime.UtcNow
        };

        study.Id = NonClinicalStudyId.New();

        return study;
    }

    /// <summary>
    /// Corrects the title. Unguarded today and wrong from S002 onward — see the
    /// remarks on <c>ClinicalStudy.Retitle</c>, which carries the reasoning.
    /// </summary>
    public void Retitle(string title)
    {
        Title = ValidatedTitle(title);
    }

    private static string ValidatedIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(
                NonClinicalStudyErrors.SponsorStudyIdentifierRequired);

        var trimmed = value.Trim();

        if (trimmed.Length > SponsorStudyIdentifierMaxLength)
            throw new DomainException(
                NonClinicalStudyErrors.SponsorStudyIdentifierTooLong);

        return trimmed;
    }

    private static string ValidatedTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(NonClinicalStudyErrors.TitleRequired);

        var trimmed = value.Trim();

        if (trimmed.Length > TitleMaxLength)
            throw new DomainException(NonClinicalStudyErrors.TitleTooLong);

        return trimmed;
    }
}
