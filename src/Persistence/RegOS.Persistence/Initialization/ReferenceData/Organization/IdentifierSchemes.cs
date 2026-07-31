using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Persistence.Initialization.ReferenceData.Organization;

internal static class IdentifierSchemeIds
{
    // Identifier schemes (8000...)
    public static readonly Guid Duns =
        Guid.Parse("80000000-0000-0000-0000-000000000001");
    public static readonly Guid Fei =
        Guid.Parse("80000000-0000-0000-0000-000000000002");
    public static readonly Guid EuOrgId =
        Guid.Parse("80000000-0000-0000-0000-000000000003");
    public static readonly Guid SplId =
        Guid.Parse("80000000-0000-0000-0000-000000000004");
}

/// <summary>
/// The registries that issue organization and site identifiers.
/// </summary>
/// <remarks>
/// World facts, not a tenant's list — a DUNS number does not become a different
/// scheme because one tenant thinks about it differently. Seeded globally and
/// unfiltered, like <c>Country</c> and <c>Authority</c>.
/// </remarks>
internal static class IdentifierSchemes
{
    public static readonly IReadOnlyList<IdentifierScheme> Data =
    [
        IdentifierScheme.Create(
            new IdentifierSchemeId(IdentifierSchemeIds.Duns),
            "DUNS",
            "Data Universal Numbering System",
            "Dun & Bradstreet"),

        IdentifierScheme.Create(
            new IdentifierSchemeId(IdentifierSchemeIds.Fei),
            "FEI",
            "FDA Establishment Identifier",
            "US Food and Drug Administration"),

        IdentifierScheme.Create(
            new IdentifierSchemeId(IdentifierSchemeIds.EuOrgId),
            "EU-ORG-ID",
            "EU Organisation Identifier (OMS)",
            "European Medicines Agency"),

        IdentifierScheme.Create(
            new IdentifierSchemeId(IdentifierSchemeIds.SplId),
            "SPL-ID",
            "Structured Product Labeling Establishment Identifier",
            "US Food and Drug Administration"),
    ];
}
