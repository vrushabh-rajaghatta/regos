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

        // Build the v1 blueprint on a draft, then publish (freeze) it. A thin
        // CTD slice for now — required documents and validation rules are added
        // in later stories, which re-seed this blueprint as it grows.
        var v1 = template.StartDraftVersion();

        template.AddSection(
            "M1", "Administrative Information and Prescribing Information", null, 1);
        template.AddSection(
            "M2", "Common Technical Document Summaries", null, 2);
        var m3 = template.AddSection("M3", "Quality", null, 3);
        template.AddSection("3.2.S", "Drug Substance", m3.Id, 1);
        template.AddSection("3.2.P", "Drug Product", m3.Id, 2);
        template.AddSection("M4", "Nonclinical Study Reports", null, 4);
        template.AddSection("M5", "Clinical Study Reports", null, 5);

        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        return template;
    }
}
