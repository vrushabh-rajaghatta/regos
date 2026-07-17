using RegOS.ReferenceData.Domain.Regulatory.Authority;

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
            throw new ArgumentException(
                SubmissionTypeErrors.CodeRequired,
                nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                SubmissionTypeErrors.NameRequired,
                nameof(name));

        if (authorityId == default)
            throw new ArgumentException(
                SubmissionTypeErrors.AuthorityRequired,
                nameof(authorityId));

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
