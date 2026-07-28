namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class DocumentTypeIds
{
    // Document Types (5000...)
    public static readonly Guid Cer =
        Guid.Parse("50000000-0000-0000-0000-000000000001");
    public static readonly Guid Rmf =
        Guid.Parse("50000000-0000-0000-0000-000000000002");
    public static readonly Guid Ssd =
        Guid.Parse("50000000-0000-0000-0000-000000000003");
    public static readonly Guid Ifu =
        Guid.Parse("50000000-0000-0000-0000-000000000004");
    public static readonly Guid Lbl =
        Guid.Parse("50000000-0000-0000-0000-000000000005");
    public static readonly Guid Rmp =
        Guid.Parse("50000000-0000-0000-0000-000000000006");
    public static readonly Guid Tvr =
        Guid.Parse("50000000-0000-0000-0000-000000000007");
    public static readonly Guid Val =
        Guid.Parse("50000000-0000-0000-0000-000000000008");

    // CTD / pharma document types (thin FDA IND slice).
    public static readonly Guid CoverLetter =
        Guid.Parse("50000000-0000-0000-0000-000000000009");
    public static readonly Guid FormFda1571 =
        Guid.Parse("50000000-0000-0000-0000-00000000000a");
    public static readonly Guid InvestigatorsBrochure =
        Guid.Parse("50000000-0000-0000-0000-00000000000b");
    public static readonly Guid NonclinicalOverview =
        Guid.Parse("50000000-0000-0000-0000-00000000000c");
    public static readonly Guid ClinicalOverview =
        Guid.Parse("50000000-0000-0000-0000-00000000000d");
    public static readonly Guid DrugSubstanceSummary =
        Guid.Parse("50000000-0000-0000-0000-00000000000e");
    public static readonly Guid DrugProductSummary =
        Guid.Parse("50000000-0000-0000-0000-00000000000f");

    // Additional IND artifacts (STORY-006 — full FDA IND blueprint).
    public static readonly Guid FormFda1572 =
        Guid.Parse("50000000-0000-0000-0000-000000000010");
    public static readonly Guid FormFda3674 =
        Guid.Parse("50000000-0000-0000-0000-000000000011");
    public static readonly Guid StudyProtocol =
        Guid.Parse("50000000-0000-0000-0000-000000000012");
    public static readonly Guid QualityOverallSummary =
        Guid.Parse("50000000-0000-0000-0000-000000000013");
    public static readonly Guid NonclinicalSummary =
        Guid.Parse("50000000-0000-0000-0000-000000000014");
    public static readonly Guid ClinicalSummary =
        Guid.Parse("50000000-0000-0000-0000-000000000015");
}
