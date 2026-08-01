using RegOS.ReferenceData.Domain.Regulatory.Correspondence;

using CorrespondenceTypeEntity =
    RegOS.ReferenceData.Domain.Regulatory.Correspondence.CorrespondenceType;

namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class CorrespondenceTypes
{
    // Deliberately short. Eight kinds cover what a regulatory user files
    // today; the vocabulary is reference data precisely so the ninth does not
    // need a deployment.
    public static IReadOnlyList<CorrespondenceTypeEntity> Data =>
    [
        CorrespondenceTypeEntity.Create(
            new CorrespondenceTypeId(CorrespondenceTypeIds.InformationRequest),
            "INFORMATION_REQUEST",
            "Information Request"),
        CorrespondenceTypeEntity.Create(
            new CorrespondenceTypeId(CorrespondenceTypeIds.DeficiencyLetter),
            "DEFICIENCY_LETTER",
            "Deficiency Letter"),
        CorrespondenceTypeEntity.Create(
            new CorrespondenceTypeId(CorrespondenceTypeIds.ApprovalLetter),
            "APPROVAL_LETTER",
            "Approval Letter"),
        CorrespondenceTypeEntity.Create(
            new CorrespondenceTypeId(CorrespondenceTypeIds.Acknowledgement),
            "ACKNOWLEDGEMENT",
            "Acknowledgement"),
        CorrespondenceTypeEntity.Create(
            new CorrespondenceTypeId(CorrespondenceTypeIds.MeetingRequest),
            "MEETING_REQUEST",
            "Meeting Request"),
        CorrespondenceTypeEntity.Create(
            new CorrespondenceTypeId(CorrespondenceTypeIds.MeetingMinutes),
            "MEETING_MINUTES",
            "Meeting Minutes"),
        CorrespondenceTypeEntity.Create(
            new CorrespondenceTypeId(CorrespondenceTypeIds.ResponseToAuthority),
            "RESPONSE",
            "Response to Authority"),
        CorrespondenceTypeEntity.Create(
            new CorrespondenceTypeId(CorrespondenceTypeIds.GeneralCorrespondence),
            "GENERAL",
            "General Correspondence")
    ];
}
