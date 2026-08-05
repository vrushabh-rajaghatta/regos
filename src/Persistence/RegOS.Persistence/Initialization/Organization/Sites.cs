using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Persistence.Initialization.ReferenceData;
using RegOS.Persistence.Initialization.ReferenceData.Organization;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Initialization.Organization;

/// <summary>
/// Three places the demo manufacturer operates.
/// </summary>
/// <remarks>
/// <b>Demonstration seed data only.</b> These are invented sites at invented
/// addresses with invented registry numbers; nothing here is a real facility and
/// no FEI or DUNS below is issued.
/// <para>
/// <b>They exist because EPIC-010c S001 found the registry empty.</b> Sites have
/// been an aggregate root since EPIC-016 and nothing ever seeded one, so
/// <em>"which sites make this product?"</em> had no answer it could give on a
/// fresh database — the site picker offered nothing to pick. Found by the
/// browser proof rather than by review, which is the same way EPIC-022 S002
/// found that neither EU market had an authority.
/// </para>
/// <para>
/// <b>Three, chosen so the question stays interesting.</b> A finished-product
/// plant, a testing laboratory and an API plant, in three countries and of
/// two site types — enough for a market to be supplied from more than one
/// place, which is the case the divergence in S004 is about. A single site
/// would make every answer trivially the same.
/// </para>
/// </remarks>
internal static class Sites
{
    /// <summary>
    /// <b>The acting tenant, which is Demo MAH Ltd. and not Demo Manufacturer
    /// Ltd.</b>
    /// </summary>
    /// <remarks>
    /// <b>The name is a decoy and it cost a debugging round.</b> Three demo
    /// organizations exist and the obvious one to hang a plant off is called
    /// <em>Demo Manufacturer Ltd.</em> — but <c>dev@regos.local</c> belongs to
    /// tenant <c>…0003</c>, <em>Demo MAH Ltd.</em>, and every browser spec acts
    /// as that tenant. Sites seeded under the manufacturer are real rows that
    /// **nobody who logs in can see**, because the query filter is fail-closed
    /// and working exactly as designed (ADR-031).
    /// <para>
    /// It is also not wrong in the domain: a marketing authorisation holder
    /// owning its own plants is ordinary. The lesson is narrower — <b>seed to
    /// the tenant that logs in, not to the organization whose name matches the
    /// concept</b>.
    /// </para>
    /// </remarks>
    private static readonly TenantId ActingTenant =
        new(Platform.TenantIds.DemoMarketingAuthorizationHolder);

    private static readonly OrganizationId Owner =
        new(OrganizationIds.DemoMarketingAuthorizationHolder);

    public static IReadOnlyList<OrganizationSite> Data
    {
        get
        {
            var cologne = OrganizationSite.Create(
                new OrganizationSiteId(SiteIds.CologneWorks),
                ActingTenant,
                Owner,
                "Demo Pharma Werk Köln",
                OrganizationSiteType.Manufacturing,
                PostalAddress.Create(
                    new CountryId(GeographyAndRegulatoryIds.Germany),
                    line1: "Industriestraße 14",
                    city: "Köln",
                    postalCode: "50733"),
                new DateOnly(2014, 4, 1),
                nameNativeLanguage: "Demo Pharma Werk Köln");

            // A US registry number on a German plant is ordinary, not a
            // mistake: an FEI is issued by FDA to any site that supplies the US
            // market, wherever it stands.
            cologne.AddIdentifier(
                new IdentifierSchemeId(IdentifierSchemeIds.Fei), "3009876543");
            cologne.AddIdentifier(
                new IdentifierSchemeId(IdentifierSchemeIds.Duns), "315551234");

            var manchester = OrganizationSite.Create(
                new OrganizationSiteId(SiteIds.ManchesterLaboratory),
                ActingTenant,
                Owner,
                "Demo Analytical Services",
                // A laboratory, and it will still perform an operation for a
                // product — which is why the operation picker offers every
                // site rather than manufacturing ones only.
                OrganizationSiteType.Testing,
                PostalAddress.Create(
                    new CountryId(GeographyAndRegulatoryIds.UnitedKingdom),
                    line1: "Unit 7, Trafford Park",
                    city: "Manchester",
                    postalCode: "M17 1AB"),
                new DateOnly(2018, 9, 1));

            manchester.AddIdentifier(
                new IdentifierSchemeId(IdentifierSchemeIds.Duns), "225557890");

            var hyderabad = OrganizationSite.Create(
                new OrganizationSiteId(SiteIds.HyderabadApiPlant),
                ActingTenant,
                Owner,
                "Demo Active Ingredients Pvt Ltd",
                OrganizationSiteType.Manufacturing,
                PostalAddress.Create(
                    new CountryId(GeographyAndRegulatoryIds.India),
                    line1: "Plot 42, Genome Valley",
                    city: "Hyderabad",
                    stateProvince: "Telangana",
                    postalCode: "500078"),
                new DateOnly(2021, 6, 1));

            hyderabad.AddIdentifier(
                new IdentifierSchemeId(IdentifierSchemeIds.Fei), "3012345678");

            return [cologne, manchester, hyderabad];
        }
    }
}
