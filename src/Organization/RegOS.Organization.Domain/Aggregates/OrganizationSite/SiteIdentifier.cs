using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Domain.Aggregates.OrganizationSite;

/// <summary>
/// One registry's identifier for a site — an FEI, a DUNS number — as the pair
/// <em>scheme + value</em>.
/// </summary>
/// <remarks>
/// A collection from day one rather than a single field, because the real world
/// already says so: a US manufacturing site has both an FEI and a DUNS number
/// today, and they are peers rather than one being an alternative to the other.
/// Modelling one identifier would be shipping a model everybody already knows
/// is incomplete.
/// <para>
/// The scheme is a reference-data id rather than a bare string, so "FEI" means
/// the same registry everywhere and a new scheme is a seeded row rather than a
/// deployment.
/// </para>
/// </remarks>
public sealed class SiteIdentifier : Entity<SiteIdentifierId>
{
    public const int ValueMaxLength = 100;

    // Only the aggregate creates these.
    internal SiteIdentifier(
        SiteIdentifierId id,
        IdentifierSchemeId schemeId,
        string value)
    {
        if (schemeId == default)
            throw new DomainException(
                OrganizationSiteErrors.IdentifierSchemeRequired);

        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(
                OrganizationSiteErrors.IdentifierValueRequired);

        if (value.Trim().Length > ValueMaxLength)
            throw new DomainException(
                OrganizationSiteErrors.IdentifierValueTooLong);

        Id = id;
        SchemeId = schemeId;
        Value = value.Trim();
    }

    private SiteIdentifier()
    {
    }

    public IdentifierSchemeId SchemeId { get; private set; }

    public string Value { get; private set; } = default!;
}
