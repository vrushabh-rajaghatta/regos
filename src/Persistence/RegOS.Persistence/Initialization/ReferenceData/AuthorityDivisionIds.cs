namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class AuthorityDivisionIds
{
    // Authority Divisions (a000...). Platform-seeded rows only; a tenant's own
    // divisions get fresh guids at creation.
    public static readonly Guid FdaCder =
        Guid.Parse("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid FdaCber =
        Guid.Parse("a0000000-0000-0000-0000-000000000002");
    public static readonly Guid FdaOnd =
        Guid.Parse("a0000000-0000-0000-0000-000000000003");
    public static readonly Guid HcTpd =
        Guid.Parse("a0000000-0000-0000-0000-000000000004");
    public static readonly Guid HcBgtd =
        Guid.Parse("a0000000-0000-0000-0000-000000000005");
    public static readonly Guid TgaPrescription =
        Guid.Parse("a0000000-0000-0000-0000-000000000006");
    public static readonly Guid MhraLicensing =
        Guid.Parse("a0000000-0000-0000-0000-000000000007");
}
