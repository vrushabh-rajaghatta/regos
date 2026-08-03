using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;

using SubmissionTypeEntity =
    RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;

namespace RegOS.Persistence.Initialization.ReferenceData;

/// <summary>
/// What a regulatory activity is, for the one authority whose vocabulary this
/// project has read.
/// </summary>
/// <remarks>
/// <b>Every row here carries a token, and that is the seeding rule.</b> The
/// tokens are Level 3 evidence — read out of FDA's published worked examples
/// #21–#24 and recorded in
/// <c>docs/evidence/EPIC-007a/ectd-mapping.md</c> — and no parser we own can
/// check them (evidence E12: the DTD types the attribute <c>CDATA</c>, so
/// <c>fdast99</c> is perfectly valid XML and rejected only at the gateway).
/// <para>
/// <b>FDA's vocabulary is larger than this, and the rest is deliberately
/// absent.</b> The <i>Submission Types and Subtypes</i> document names the
/// activities in readable prose but never prints the tokens, so seeding a row
/// we cannot render would put a value in front of a user that no package can
/// carry. A row arrives when its token does.
/// </para>
/// </remarks>
internal static class SubmissionTypes
{
    public static IReadOnlyList<SubmissionTypeEntity> Data =>
    [
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaOriginalApplication),
            "FDA_ORIGINAL_APPLICATION",
            "Original Application",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "fdast1"),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaAnnualReport),
            "FDA_ANNUAL_REPORT",
            "Annual Report",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "fdast5"),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaIndSafetyReport),
            "FDA_IND_SAFETY_REPORT",
            "IND Safety Report",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "fdast9")
    ];
}
