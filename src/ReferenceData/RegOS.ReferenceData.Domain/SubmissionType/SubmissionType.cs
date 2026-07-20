using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.SubmissionType;

public sealed class SubmissionType
{
    private SubmissionType()
    {
    }

    public SubmissionTypeId Id { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public AuthorityId AuthorityId { get; private set; }

    public bool IsActive { get; private set; }

    public static SubmissionType Create(
        SubmissionTypeId id,
        string code,
        string name,
        AuthorityId authorityId)
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
            AuthorityId = authorityId,
            IsActive = true
        };
    }

    public static SubmissionType Create(
        string code,
        string name,
        AuthorityId authorityId)
        => Create(SubmissionTypeId.New(), code, name, authorityId);
}
