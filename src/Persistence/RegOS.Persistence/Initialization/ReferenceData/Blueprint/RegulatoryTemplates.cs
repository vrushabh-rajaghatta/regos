using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;

namespace RegOS.Persistence.Initialization.ReferenceData.Blueprint;

internal static class RegulatoryTemplates
{
    // Deterministic ids from RegulatoryTemplateIds; the authority and
    // submission-type references reuse the ids already seeded elsewhere.
    public static IReadOnlyList<RegulatoryTemplate> Data =>
    [
        BuildFdaIndCtd()
    ];

    private static RegulatoryTemplate BuildFdaIndCtd()
    {
        var template = RegulatoryTemplate.Create(
            new RegulatoryTemplateId(RegulatoryTemplateIds.FdaIndCtd),
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            new SubmissionTypeId(SubmissionTypeIds.FdaInd),
            "ICH eCTD / FDA");

        // A published v1 shell — its sections, required documents and
        // validation rules are added in later stories.
        var v1 = template.StartDraftVersion();
        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        return template;
    }
}
