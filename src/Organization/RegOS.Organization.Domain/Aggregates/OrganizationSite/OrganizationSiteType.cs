namespace RegOS.Organization.Domain.Aggregates.OrganizationSite;

/// <summary>
/// What a site is for.
/// </summary>
/// <remarks>
/// A closed enum rather than reference data: it drives behaviour. Only a
/// manufacturing site can be named on a licence as an approved manufacturer,
/// and only a testing site can be a release-testing location — rules that live
/// in code, so the vocabulary they branch on does too. Contrast
/// <c>IdentifierScheme</c>, which nothing branches on and is therefore data.
/// </remarks>
public enum OrganizationSiteType
{
    Manufacturing = 1,
    Packaging = 2,
    Testing = 3,
    Storage = 4,

    /// <summary>An office of a health authority — where correspondence goes.</summary>
    AuthorityOffice = 5,

    /// <summary>A sponsor or partner's own office, not a production location.</summary>
    Office = 6,
}
