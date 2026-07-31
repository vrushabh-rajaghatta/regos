using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Organization.Application.Commands.CreateOrganizationSite;

/// <param name="StatusDate">
/// The business date the site opened — supplied rather than read from the clock,
/// so a site recorded today can say it has operated since 2014.
/// </param>
/// <param name="Identifiers">
/// Zero or more registry identifiers. A US plant routinely arrives with both an
/// FEI and a DUNS number, so they are recorded together with the site rather
/// than added one call at a time.
/// </param>
public sealed record CreateOrganizationSiteCommand(
    OrganizationId OrganizationId,
    string Name,
    OrganizationSiteType Type,
    CountryId CountryId,
    DateOnly StatusDate,
    string? NameNativeLanguage = null,
    string? AddressLine1 = null,
    string? AddressLine2 = null,
    string? AddressLine3 = null,
    string? City = null,
    string? StateProvince = null,
    string? PostalCode = null,
    string? Email = null,
    string? Phone = null,
    IReadOnlyList<SiteIdentifierInput>? Identifiers = null);

public sealed record SiteIdentifierInput(
    IdentifierSchemeId SchemeId,
    string Value);
