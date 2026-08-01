namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class CorrespondenceTypeIds
{
    // Correspondence Types (9000...). Global, not authority-scoped — every
    // authority sends information requests under local names.
    public static readonly Guid InformationRequest =
        Guid.Parse("90000000-0000-0000-0000-000000000001");
    public static readonly Guid DeficiencyLetter =
        Guid.Parse("90000000-0000-0000-0000-000000000002");
    public static readonly Guid ApprovalLetter =
        Guid.Parse("90000000-0000-0000-0000-000000000003");
    public static readonly Guid Acknowledgement =
        Guid.Parse("90000000-0000-0000-0000-000000000004");
    public static readonly Guid MeetingRequest =
        Guid.Parse("90000000-0000-0000-0000-000000000005");
    public static readonly Guid MeetingMinutes =
        Guid.Parse("90000000-0000-0000-0000-000000000006");
    public static readonly Guid ResponseToAuthority =
        Guid.Parse("90000000-0000-0000-0000-000000000007");
    public static readonly Guid GeneralCorrespondence =
        Guid.Parse("90000000-0000-0000-0000-000000000008");
}
