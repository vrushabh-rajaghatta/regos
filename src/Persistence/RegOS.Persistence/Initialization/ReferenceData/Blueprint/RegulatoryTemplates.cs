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

        // Build the v1 blueprint on a draft, then publish (freeze) it. A
        // representative CTD skeleton for FDA IND: the harmonized modules (2–5)
        // to their standard section families, the FDA regional Module 1 to its
        // IND essentials — one level below 3.2.S / 3.2.P, not every CTD leaf.
        // Numbering is template data (e.g. IB at 1.13), never application logic.
        var v1 = template.StartDraftVersion();

        // ── Module 1 — Administrative Information (FDA regional) ──────────────
        var m1 = template.AddSection(
            "M1", "Administrative Information and Prescribing Information", null, 1);
        var forms = template.AddSection("1.1", "Forms", m1.Id, 1);
        var coverLetter = template.AddSection("1.2", "Cover Letter", m1.Id, 2);
        template.AddSection("1.3", "Administrative Information", m1.Id, 3);
        template.AddSection("1.4", "References", m1.Id, 4);
        var ib = template.AddSection("1.13", "Investigator's Brochure", m1.Id, 5);
        template.AddSection("1.14", "Labeling", m1.Id, 6);

        // ── Module 2 — CTD Summaries ─────────────────────────────────────────
        var m2 = template.AddSection(
            "M2", "Common Technical Document Summaries", null, 2);
        var qos = template.AddSection("2.3", "Quality Overall Summary", m2.Id, 1);
        var nonclinicalOverview =
            template.AddSection("2.4", "Nonclinical Overview", m2.Id, 2);
        var clinicalOverview =
            template.AddSection("2.5", "Clinical Overview", m2.Id, 3);
        var nonclinicalSummary = template.AddSection(
            "2.6", "Nonclinical Written and Tabulated Summaries", m2.Id, 4);
        var clinicalSummary =
            template.AddSection("2.7", "Clinical Summary", m2.Id, 5);

        // ── Module 3 — Quality ───────────────────────────────────────────────
        var m3 = template.AddSection("M3", "Quality", null, 3);
        var substance = template.AddSection("3.2.S", "Drug Substance", m3.Id, 1);
        template.AddSection("3.2.S.1", "General Information", substance.Id, 1);
        template.AddSection("3.2.S.2", "Manufacture", substance.Id, 2);
        template.AddSection("3.2.S.3", "Characterisation", substance.Id, 3);
        template.AddSection("3.2.S.4", "Control of Drug Substance", substance.Id, 4);
        template.AddSection("3.2.S.5", "Reference Standards or Materials", substance.Id, 5);
        template.AddSection("3.2.S.6", "Container Closure System", substance.Id, 6);
        var sStability = template.AddSection("3.2.S.7", "Stability", substance.Id, 7);

        var product = template.AddSection("3.2.P", "Drug Product", m3.Id, 2);
        template.AddSection(
            "3.2.P.1", "Description and Composition of the Drug Product", product.Id, 1);
        template.AddSection("3.2.P.2", "Pharmaceutical Development", product.Id, 2);
        template.AddSection("3.2.P.3", "Manufacture", product.Id, 3);
        template.AddSection("3.2.P.4", "Control of Excipients", product.Id, 4);
        template.AddSection("3.2.P.5", "Control of Drug Product", product.Id, 5);
        template.AddSection("3.2.P.6", "Reference Standards or Materials", product.Id, 6);
        template.AddSection("3.2.P.7", "Container Closure System", product.Id, 7);
        var pStability = template.AddSection("3.2.P.8", "Stability", product.Id, 8);

        // ── Module 4 — Nonclinical Study Reports ─────────────────────────────
        var m4 = template.AddSection("M4", "Nonclinical Study Reports", null, 4);
        template.AddSection("4.2.1", "Pharmacology", m4.Id, 1);
        template.AddSection("4.2.2", "Pharmacokinetics", m4.Id, 2);
        template.AddSection("4.2.3", "Toxicology", m4.Id, 3);

        // ── Module 5 — Clinical Study Reports ────────────────────────────────
        var m5 = template.AddSection("M5", "Clinical Study Reports", null, 5);
        template.AddSection("5.2", "Tabular Listing of All Clinical Studies", m5.Id, 1);
        var clinicalReports =
            template.AddSection("5.3", "Clinical Study Reports", m5.Id, 2);

        // ── The documents each section expects, typed by DocumentType ────────
        template.AddRequiredDocument(
            coverLetter.Id, new DocumentTypeId(DocumentTypeIds.CoverLetter), true, 1);
        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.FormFda1571), true, 1);
        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.FormFda1572), true, 2);
        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.FormFda3674), true, 3);
        template.AddRequiredDocument(
            ib.Id, new DocumentTypeId(DocumentTypeIds.InvestigatorsBrochure), true, 1);
        template.AddRequiredDocument(
            qos.Id, new DocumentTypeId(DocumentTypeIds.QualityOverallSummary), true, 1);
        template.AddRequiredDocument(
            nonclinicalOverview.Id,
            new DocumentTypeId(DocumentTypeIds.NonclinicalOverview), true, 1);
        template.AddRequiredDocument(
            clinicalOverview.Id,
            new DocumentTypeId(DocumentTypeIds.ClinicalOverview), true, 1);
        template.AddRequiredDocument(
            nonclinicalSummary.Id,
            new DocumentTypeId(DocumentTypeIds.NonclinicalSummary), true, 1);
        template.AddRequiredDocument(
            clinicalSummary.Id,
            new DocumentTypeId(DocumentTypeIds.ClinicalSummary), true, 1);
        template.AddRequiredDocument(
            substance.Id, new DocumentTypeId(DocumentTypeIds.DrugSubstanceSummary), true, 1);
        template.AddRequiredDocument(
            product.Id, new DocumentTypeId(DocumentTypeIds.DrugProductSummary), true, 1);
        template.AddRequiredDocument(
            clinicalReports.Id, new DocumentTypeId(DocumentTypeIds.StudyProtocol), true, 1);

        // ── Validation rules — checkable constraints, data only ──────────────
        template.AddValidationRule(
            "FDA-IND-PDF",
            ValidationRuleType.FileFormat,
            ValidationSeverity.Error,
            "All submission documents must be provided as PDF.",
            sectionId: null,
            parameters: "pdf",
            order: 1);
        template.AddValidationRule(
            "FDA-IND-1.1-FORMS-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Error,
            "Module 1.1 (Forms) must contain the required IND forms.",
            sectionId: forms.Id,
            parameters: null,
            order: 2);
        template.AddValidationRule(
            "FDA-IND-3.2.S.7-STABILITY-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Warning,
            "Drug Substance stability data (3.2.S.7) is expected.",
            sectionId: sStability.Id,
            parameters: null,
            order: 3);
        template.AddValidationRule(
            "FDA-IND-3.2.P.8-STABILITY-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Warning,
            "Drug Product stability data (3.2.P.8) is expected.",
            sectionId: pStability.Id,
            parameters: null,
            order: 4);

        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        return template;
    }
}
