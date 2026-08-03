using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.ApplicationType;

/// <summary>
/// What kind of application this is — IND, NDA, 510(k), CTA, ARTG inclusion.
/// </summary>
/// <remarks>
/// <b>This is eCTD's <c>application-type</c>, not its <c>submission-type</c></b>
/// (evidence E11). It was called <c>SubmissionType</c> and hung off
/// <c>Submission</c> until EPIC-007a S001; the value is invariant across every
/// submission in an application, which is what places it on the aggregate root.
/// <para>
/// eCTD's actual <c>submission-type</c> — original-application, annual-report,
/// IND safety report — classifies a regulatory activity, not an application.
/// It is a separate concept and does not belong here.
/// </para>
/// </remarks>
public sealed class ApplicationType
{
    private ApplicationType()
    {
    }

    public ApplicationTypeId Id { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    /// <summary>
    /// The value this appears as in <c>us-regional.xml</c>'s
    /// <c>application-type</c> — <c>fdaat4</c> for an IND — or null when the
    /// authority's wire vocabulary is not modelled.
    /// </summary>
    /// <remarks>
    /// Stored rather than derived, and null meaning <i>not in evidence</i>, for
    /// the reasons set out on <see cref="SubmissionType.SubmissionType.Token"/>.
    /// <para>
    /// <c>FDA_IND</c> is the only row that has one today. A <c>TGA_ARTG</c>
    /// application cannot be rendered until someone reads the TGA
    /// specification — and neither can an <c>FDA_NDA</c>, for the different
    /// reason that FDA prints its tokens only inside worked examples. The
    /// failure names the row, so the two are told apart by what is missing
    /// rather than by a guess about why.
    /// </para>
    /// </remarks>
    public string? Token { get; private set; }

    public AuthorityId AuthorityId { get; private set; }

    public bool IsActive { get; private set; }

    public static ApplicationType Create(
        ApplicationTypeId id,
        string code,
        string name,
        AuthorityId authorityId,
        string? token = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(ApplicationTypeErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ApplicationTypeErrors.NameRequired);

        if (authorityId == default)
            throw new DomainException(ApplicationTypeErrors.AuthorityRequired);

        return new ApplicationType
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

    public static ApplicationType Create(
        string code,
        string name,
        AuthorityId authorityId,
        string? token = null)
        => Create(ApplicationTypeId.New(), code, name, authorityId, token);

    /// <summary>
    /// Records the value this type appears as on the wire, or clears it.
    /// </summary>
    /// <remarks>
    /// <b>This exists here and not on the two catalogues beside it, and the
    /// asymmetry is the point</b> (ADR-018). These rows were seeded by an
    /// earlier story and predate the token, so a database that has already been
    /// upgraded has no other way to acquire one — and a fresh clone and an
    /// upgraded database must converge. <c>SubmissionType</c> and
    /// <c>SubmissionSubType</c> are seeded with their tokens from the first row
    /// they ever have, so neither has demonstrated the same need. Symmetry is
    /// not a demonstration.
    /// </remarks>
    public void RecordToken(string? token)
        => Token = string.IsNullOrWhiteSpace(token)
            ? null
            : token.Trim().ToLowerInvariant();
}
