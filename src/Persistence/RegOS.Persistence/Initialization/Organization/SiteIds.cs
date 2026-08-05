namespace RegOS.Persistence.Initialization.Organization;

/// <summary>
/// Deterministic ids for the demo sites, so a rebuilt database keeps them.
/// </summary>
internal static class SiteIds
{
    public static readonly Guid CologneWorks =
        Guid.Parse("40000000-0000-0000-0000-000000000001");

    public static readonly Guid ManchesterLaboratory =
        Guid.Parse("40000000-0000-0000-0000-000000000002");

    public static readonly Guid HyderabadApiPlant =
        Guid.Parse("40000000-0000-0000-0000-000000000003");
}
