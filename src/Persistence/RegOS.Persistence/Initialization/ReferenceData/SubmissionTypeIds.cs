namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class SubmissionTypeIds
{
    // Submission Types — what a regulatory activity is (7000...).
    public static readonly Guid FdaOriginalApplication =
        Guid.Parse("70000000-0000-0000-0000-000000000001");
    public static readonly Guid FdaAnnualReport =
        Guid.Parse("70000000-0000-0000-0000-000000000002");
    public static readonly Guid FdaIndSafetyReport =
        Guid.Parse("70000000-0000-0000-0000-000000000003");
}
