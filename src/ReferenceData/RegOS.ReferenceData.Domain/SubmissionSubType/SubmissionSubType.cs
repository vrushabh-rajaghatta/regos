using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.SubmissionSubType;

/// <summary>
/// What a single sequence <i>does</i> to the activity it belongs to — opens it,
/// amends it, reports on it.
/// </summary>
/// <remarks>
/// <b>The third axis, and an independent one</b> (ADR-047 §6). It is not a
/// taxonomy beneath <see cref="SubmissionType.SubmissionType"/>: the type
/// classifies the activity and the sub-type classifies each sequence within it,
/// so the same sub-type appears under many types.
/// <para>
/// <b>It is not derivable from position, and the tempting rule is provably
/// wrong.</b> <em>Opener ⇒ application, continuer ⇒ amendment</em> is falsified
/// by FDA's own worked example #23 — an opening sequence whose sub-type is
/// <c>report</c> (evidence E13, corroborated independently by Table 1 of
/// <i>eCTD Submission Types and Subtypes</i>). So the user supplies it, and
/// RegOS never infers it.
/// </para>
/// </remarks>
public sealed class SubmissionSubType
{
    private SubmissionSubType()
    {
    }

    public SubmissionSubTypeId Id { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    /// <summary>
    /// The value this appears as in <c>us-regional.xml</c> — <c>fdasst4</c> —
    /// or null when the authority's wire vocabulary is not modelled.
    /// </summary>
    /// <remarks>
    /// Stored rather than derived, and null meaning <i>not modelled yet</i>, for
    /// the reasons set out on <see cref="SubmissionType.SubmissionType.Token"/>.
    /// </remarks>
    public string? Token { get; private set; }

    public AuthorityId AuthorityId { get; private set; }

    public bool IsActive { get; private set; }

    public static SubmissionSubType Create(
        SubmissionSubTypeId id,
        string code,
        string name,
        AuthorityId authorityId,
        string? token = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(SubmissionSubTypeErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(SubmissionSubTypeErrors.NameRequired);

        if (authorityId == default)
            throw new DomainException(SubmissionSubTypeErrors.AuthorityRequired);

        return new SubmissionSubType
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Token = string.IsNullOrWhiteSpace(token)
                ? null
                : token.Trim().ToLowerInvariant(),
            AuthorityId = authorityId,
            IsActive = true
        };
    }

    public static SubmissionSubType Create(
        string code,
        string name,
        AuthorityId authorityId,
        string? token = null)
        => Create(SubmissionSubTypeId.New(), code, name, authorityId, token);
}
