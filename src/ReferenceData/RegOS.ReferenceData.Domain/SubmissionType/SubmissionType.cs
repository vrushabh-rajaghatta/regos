using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.SubmissionType;

/// <summary>
/// What a regulatory activity <i>is</i> — an original application, an annual
/// report, an IND safety report.
/// </summary>
/// <remarks>
/// <b>This is eCTD's <c>submission-type</c>, and the name was reserved for it</b>
/// (ADR-050 §4). Until EPIC-007a S001 the name was used for what eCTD calls
/// <c>application-type</c>; that catalogue is now
/// <see cref="ApplicationType.ApplicationType"/>, and this is the concept the
/// name always meant.
/// <para>
/// It classifies an <em>activity</em>, not a sequence and not an application.
/// The distinction is the whole of evidence E11: an IND is one application, and
/// over its life it carries many activities — the original submission, each
/// annual report, each safety report — every one of which spans several
/// sequences.
/// </para>
/// </remarks>
public sealed class SubmissionType
{
    private SubmissionType()
    {
    }

    public SubmissionTypeId Id { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    /// <summary>
    /// The value this appears as in <c>us-regional.xml</c> — <c>fdast1</c>,
    /// <c>fdast5</c> — or null when the authority's wire vocabulary is not
    /// modelled.
    /// </summary>
    /// <remarks>
    /// <b>Stored, never derived from <see cref="Code"/></b> (evidence E8). The
    /// readable phrase exists only in an XML comment beside the token; there is
    /// no rule connecting the two, and any rule we invented would be ours.
    /// <para>
    /// <b>Null has one precise meaning: the token for this row is not in
    /// evidence.</b> Not "unknown", and emphatically not "work it out". A
    /// package that needs an absent token fails by name rather than rendering a
    /// guess.
    /// </para>
    /// <para>
    /// The scope is the <em>row</em>, not the authority — a smaller claim, and
    /// the true one. It is tempting to read a null as "nobody here has read the
    /// TGA specification", and for TGA that happens to be why; but FDA's own NDA
    /// and 510(k) tokens are equally absent, because FDA publishes the readable
    /// phrases in prose and prints the tokens only inside worked examples.
    /// </para>
    /// <para>
    /// The DTD types this attribute <c>CDATA #REQUIRED</c> — required but
    /// <em>not</em> enumerated (evidence E12) — so a wrong token is perfectly
    /// DTD-valid and rejected only at the gateway. Nothing downstream of RegOS
    /// will catch a typo, which is why the vocabulary is curated reference data
    /// rather than a string on a submission.
    /// </para>
    /// </remarks>
    public string? Token { get; private set; }

    public AuthorityId AuthorityId { get; private set; }

    public bool IsActive { get; private set; }

    public static SubmissionType Create(
        SubmissionTypeId id,
        string code,
        string name,
        AuthorityId authorityId,
        string? token = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(SubmissionTypeErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(SubmissionTypeErrors.NameRequired);

        if (authorityId == default)
            throw new DomainException(SubmissionTypeErrors.AuthorityRequired);

        return new SubmissionType
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            // Blank collapses to null rather than to "": an empty token would
            // render as an empty attribute, which is a different lie from a
            // missing one.
            Token = string.IsNullOrWhiteSpace(token)
                ? null
                : token.Trim().ToLowerInvariant(),
            AuthorityId = authorityId,
            IsActive = true
        };
    }

    public static SubmissionType Create(
        string code,
        string name,
        AuthorityId authorityId,
        string? token = null)
        => Create(SubmissionTypeId.New(), code, name, authorityId, token);
}
