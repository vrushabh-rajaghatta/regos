using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
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
        // CTD slice for now — validation rules are added in a later story, which
        // re-seeds this blueprint as it grows.
        var v1 = template.StartDraftVersion();

        var m1 = template.AddSection(
            "M1", "Administrative Information and Prescribing Information", null, 1);
        var m2 = template.AddSection(
            "M2", "Common Technical Document Summaries", null, 2);
        var m3 = template.AddSection("M3", "Quality", null, 3);
        var substance = template.AddSection("3.2.S", "Drug Substance", m3.Id, 1);
        var product = template.AddSection("3.2.P", "Drug Product", m3.Id, 2);
        template.AddSection("M4", "Nonclinical Study Reports", null, 4);
        template.AddSection("M5", "Clinical Study Reports", null, 5);

        // The documents each section expects, typed by DocumentType.
        template.AddRequiredDocument(
            m1.Id, new DocumentTypeId(DocumentTypeIds.CoverLetter), true, 1);
        template.AddRequiredDocument(
            m1.Id, new DocumentTypeId(DocumentTypeIds.FormFda1571), true, 2);
        template.AddRequiredDocument(
            m2.Id, new DocumentTypeId(DocumentTypeIds.NonclinicalOverview), true, 1);
        template.AddRequiredDocument(
            m2.Id, new DocumentTypeId(DocumentTypeIds.ClinicalOverview), true, 2);
        template.AddRequiredDocument(
            substance.Id, new DocumentTypeId(DocumentTypeIds.DrugSubstanceSummary), true, 1);
        template.AddRequiredDocument(
            product.Id, new DocumentTypeId(DocumentTypeIds.DrugProductSummary), true, 1);

        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        return template;
    }
}
