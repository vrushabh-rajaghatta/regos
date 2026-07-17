namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class GeographyAndRegulatoryIds
{
    // Geography (1000...)
    public static readonly Guid UnitedStates =
        Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid Canada =
        Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid UnitedKingdom =
        Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid Germany =
        Guid.Parse("10000000-0000-0000-0000-000000000004");
    public static readonly Guid France =
        Guid.Parse("10000000-0000-0000-0000-000000000005");
    public static readonly Guid Japan =
        Guid.Parse("10000000-0000-0000-0000-000000000006");
    public static readonly Guid Australia =
        Guid.Parse("10000000-0000-0000-0000-000000000007");
    public static readonly Guid India =
        Guid.Parse("10000000-0000-0000-0000-000000000008");

    // Regulatory (2000...)
    public static readonly Guid FDA =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid HealthCanada =
        Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid MHRA =
        Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid PMDA =
        Guid.Parse("20000000-0000-0000-0000-000000000004");
    public static readonly Guid TGA =
        Guid.Parse("20000000-0000-0000-0000-000000000005");
    public static readonly Guid CDSCO =
        Guid.Parse("20000000-0000-0000-0000-000000000006");
}
