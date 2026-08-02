using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Regulatory.Correspondence;

/// <summary>
/// What kind of communication a piece of correspondence is — an information
/// request, a deficiency letter, an approval letter.
/// </summary>
/// <remarks>
/// Reference data rather than an enum on ADR-038's test: nothing branches on
/// the type. It classifies and it is read; no rule reads it and behaves
/// differently. Adding <em>Refuse to File</em> must not require a deployment.
/// <para>
/// <b>Not authority-scoped, unlike <c>ApplicationType</c>.</b> An application type
/// genuinely differs per authority — an IND is not a CTA — whereas every
/// authority sends information requests and approval letters, under local
/// names. Scoping it later is additive (a nullable <c>AuthorityId</c>);
/// unscoping it would not be.
/// </para>
/// <para>
/// This is the only one of EPIC-006's eleven candidate vocabularies that S001
/// makes governed data. Correspondence format is a curated frontend constant,
/// and RIM's Action, Mode and Category are not modelled at all until something
/// asks for them — governed reference data exists because the domain needs
/// governed facts, not because dropdowns need labels (ADR-039 decision 5).
/// </para>
/// </remarks>
public sealed class CorrespondenceType
{
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 200;

    private CorrespondenceType()
    {
    }

    public CorrespondenceTypeId Id { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public bool IsActive { get; private set; }

    public static CorrespondenceType Create(
        CorrespondenceTypeId id,
        string code,
        string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(CorrespondenceTypeErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(CorrespondenceTypeErrors.NameRequired);

        return new CorrespondenceType
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            IsActive = true
        };
    }

    public static CorrespondenceType Create(string code, string name)
        => Create(CorrespondenceTypeId.New(), code, name);
}
