using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.Organization;

public sealed class OrganizationIdentifierId : StronglyTypedId
{
    public OrganizationIdentifierId(Guid value) : base(value)
    {
    }

    public static OrganizationIdentifierId New() => new(Guid.NewGuid());
}

/// <summary>
/// One registry's identifier for a company — a DUNS number, a VAT number, a
/// company registration number — as the pair <em>scheme + value</em>.
/// </summary>
/// <remarks>
/// A collection because companies routinely hold several at once, and they are
/// peers rather than alternatives.
/// <para>
/// <b>Deliberately duplicates <c>SiteIdentifier</c></b> rather than sharing a
/// base type. This is the <em>second</em> occurrence of scheme-plus-value, and
/// the Rule of Three (ADR-018) says wait: we do not yet know whether the third
/// consumer — likely the market-local product tier in EPIC-017 — wants the same
/// abstraction. Reuse the concept, duplicate the code, extract when the third
/// arrives. <b>See <c>SiteIdentifier</c>; extract on the third occurrence.</b>
/// </para>
/// </remarks>
public sealed class OrganizationIdentifier : Entity<OrganizationIdentifierId>
{
    public const int ValueMaxLength = 100;

    // Only the aggregate creates these.
    internal OrganizationIdentifier(
        OrganizationIdentifierId id,
        IdentifierSchemeId schemeId,
        string value)
    {
        if (schemeId == default)
            throw new DomainException(
                OrganizationErrors.IdentifierSchemeRequired);

        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(
                OrganizationErrors.IdentifierValueRequired);

        if (value.Trim().Length > ValueMaxLength)
            throw new DomainException(
                OrganizationErrors.IdentifierValueTooLong);

        Id = id;
        SchemeId = schemeId;
        Value = value.Trim();
    }

    private OrganizationIdentifier()
    {
    }

    public IdentifierSchemeId SchemeId { get; private set; }

    public string Value { get; private set; } = default!;
}
