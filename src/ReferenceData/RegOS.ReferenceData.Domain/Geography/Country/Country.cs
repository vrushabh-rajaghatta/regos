using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Geography.Country;

/// <summary>
/// A jurisdiction RegOS holds regulatory records for.
/// </summary>
/// <remarks>
/// <b>Flat master data, and it stays that way</b>
/// (<see href="../../../../docs/adr/ADR-043-strongly-typed-identity.md">ADR-043</see>
/// §2): deterministic ids, no lifecycle, no behaviour beyond <see cref="Create"/>.
/// The collections EPIC-022 adds are owned <em>values</em> — no identity, no
/// lifecycle, replaced whole — which is not the same as children, so
/// <see cref="CountryId"/> remains a record struct.
/// <para>
/// <b>The falsifier, named rather than left to argument:</b> if EPIC-012 gives a
/// country a lifecycle — active/inactive, merged, renamed — it becomes
/// <c>Entity&lt;CountryId&gt;</c> and the identity conversion comes with it.
/// </para>
/// <para>
/// <b>Two names and two codes, for two audiences.</b> <see cref="Name"/> and
/// <see cref="Code"/> are what a person reads and picks from a list.
/// <see cref="IsoName"/> and <see cref="IsoAlpha3Code"/> are what a
/// machine-readable submission has to carry, and they are frequently not the
/// same string — <em>"United Kingdom"</em> against <em>"United Kingdom of Great
/// Britain and Northern Ireland"</em>. Neither can be derived from the other in
/// either direction, which is why both are stored.
/// </para>
/// </remarks>
public sealed class Country
{
    public const int IsoAlpha3CodeLength = 3;
    public const int IsoNameMaxLength = 200;

    private Country()
    {
    }

    public CountryId Id { get; private set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g. US, IN, JP).
    /// </summary>
    public string Code { get; private set; } = default!;

    /// <summary>
    /// ISO 3166-1 alpha-3 country code (e.g. USA, IND, JPN).
    /// </summary>
    /// <remarks>
    /// <b>Not derivable from <see cref="Code"/>.</b> The two registers are
    /// assigned separately — <c>DE → DEU</c> looks mechanical, <c>GB → GBR</c>
    /// less so, and elsewhere no rule connects the pair at all. xEVMPD and IDMP
    /// name countries in alpha-3 (EPIC-007b), so this is the field that makes
    /// those messages renderable.
    /// </remarks>
    public string IsoAlpha3Code { get; private set; } = default!;

    /// <summary>The common name, as a person would say it.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// The official ISO 3166-1 English short name.
    /// </summary>
    /// <remarks>
    /// <em>"Korea, Republic of"</em>, not <em>"South Korea"</em>. Regulatory
    /// output requires the register's own wording, and a screen showing it would
    /// read as pedantry — which is exactly why these are two fields rather than
    /// one field with a rendering rule.
    /// </remarks>
    public string IsoName { get; private set; } = default!;

    public string? RegionCode { get; private set; }

    public static Country Create(
        CountryId id,
        string code,
        string isoAlpha3Code,
        string name,
        string isoName,
        string? regionCode = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(CountryErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(CountryErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(isoAlpha3Code))
            throw new DomainException(CountryErrors.IsoAlpha3CodeRequired);

        var alpha3 = isoAlpha3Code.Trim().ToUpperInvariant();

        // Exactly three letters, checked rather than trusted: an alpha-2 value
        // in this column would be carried into every downstream message without
        // anything noticing, and the two columns are one keystroke apart.
        if (alpha3.Length != IsoAlpha3CodeLength
            || !alpha3.All(char.IsAsciiLetterUpper))
        {
            throw new DomainException(CountryErrors.IsoAlpha3CodeMalformed);
        }

        if (string.IsNullOrWhiteSpace(isoName))
            throw new DomainException(CountryErrors.IsoNameRequired);

        if (isoName.Trim().Length > IsoNameMaxLength)
            throw new DomainException(CountryErrors.IsoNameTooLong);

        return new Country
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            IsoAlpha3Code = alpha3,
            Name = name.Trim(),
            IsoName = isoName.Trim(),
            RegionCode = regionCode
        };
    }

    public static Country Create(
        string code,
        string isoAlpha3Code,
        string name,
        string isoName,
        string? regionCode = null)
        => Create(CountryId.New(), code, isoAlpha3Code, name, isoName, regionCode);
}
