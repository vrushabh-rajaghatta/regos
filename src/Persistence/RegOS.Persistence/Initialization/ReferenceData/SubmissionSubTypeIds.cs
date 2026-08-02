namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class SubmissionSubTypeIds
{
    // Submission Sub-Types — what one sequence does to its activity (7100...).
    public static readonly Guid FdaApplication =
        Guid.Parse("71000000-0000-0000-0000-000000000001");
    public static readonly Guid FdaAmendment =
        Guid.Parse("71000000-0000-0000-0000-000000000002");
    public static readonly Guid FdaReport =
        Guid.Parse("71000000-0000-0000-0000-000000000003");
}
