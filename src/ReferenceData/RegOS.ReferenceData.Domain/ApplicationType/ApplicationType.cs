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

    public AuthorityId AuthorityId { get; private set; }

    public bool IsActive { get; private set; }

    public static ApplicationType Create(
        ApplicationTypeId id,
        string code,
        string name,
        AuthorityId authorityId)
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
            AuthorityId = authorityId,
            IsActive = true
        };
    }

    public static ApplicationType Create(
        string code,
        string name,
        AuthorityId authorityId)
        => Create(ApplicationTypeId.New(), code, name, authorityId);
}
