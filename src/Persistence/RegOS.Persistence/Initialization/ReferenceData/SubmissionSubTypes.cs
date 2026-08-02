using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionSubType;

using SubmissionSubTypeEntity =
    RegOS.ReferenceData.Domain.SubmissionSubType.SubmissionSubType;

namespace RegOS.Persistence.Initialization.ReferenceData;

/// <summary>
/// What one sequence does to the activity it belongs to.
/// </summary>
/// <remarks>
/// Seeded under the same rule as <see cref="SubmissionTypes"/>: a row exists
/// only if its wire token is in evidence.
/// <para>
/// <b>These three do not line up with the three above, and must not be made
/// to.</b> A sub-type is an independent axis (ADR-047 §6) — <c>Amendment</c>
/// appears under an original application and under an annual report alike, and
/// FDA's example #23 opens an activity with <c>Report</c> rather than
/// <c>Application</c> (evidence E13). Any table that paired them would encode a
/// rule the authority does not have.
/// </para>
/// </remarks>
internal static class SubmissionSubTypes
{
    public static IReadOnlyList<SubmissionSubTypeEntity> Data =>
    [
        SubmissionSubTypeEntity.Create(
            new SubmissionSubTypeId(SubmissionSubTypeIds.FdaApplication),
            "FDA_APPLICATION",
            "Application",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "fdasst3"),
        SubmissionSubTypeEntity.Create(
            new SubmissionSubTypeId(SubmissionSubTypeIds.FdaAmendment),
            "FDA_AMENDMENT",
            "Amendment",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "fdasst4"),
        SubmissionSubTypeEntity.Create(
            new SubmissionSubTypeId(SubmissionSubTypeIds.FdaReport),
            "FDA_REPORT",
            "Report",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "fdasst6")
    ];
}
