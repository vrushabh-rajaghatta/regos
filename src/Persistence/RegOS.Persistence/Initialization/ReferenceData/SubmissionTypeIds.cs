namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class SubmissionTypeIds
{
    // Submission Types (4000...)
    public static readonly Guid Fda510k =
        Guid.Parse("40000000-0000-0000-0000-000000000001");
    public static readonly Guid FdaDeNovo =
        Guid.Parse("40000000-0000-0000-0000-000000000002");
    public static readonly Guid FdaPma =
        Guid.Parse("40000000-0000-0000-0000-000000000003");
    public static readonly Guid CdscoImport =
        Guid.Parse("40000000-0000-0000-0000-000000000004");
    public static readonly Guid CdscoManufacturing =
        Guid.Parse("40000000-0000-0000-0000-000000000005");
    public static readonly Guid TgaArtg =
        Guid.Parse("40000000-0000-0000-0000-000000000006");
    public static readonly Guid HcMdl =
        Guid.Parse("40000000-0000-0000-0000-000000000007");

    // FDA drug (pharma) submission types.
    public static readonly Guid FdaInd =
        Guid.Parse("40000000-0000-0000-0000-000000000008");
    public static readonly Guid FdaNda =
        Guid.Parse("40000000-0000-0000-0000-000000000009");
}
