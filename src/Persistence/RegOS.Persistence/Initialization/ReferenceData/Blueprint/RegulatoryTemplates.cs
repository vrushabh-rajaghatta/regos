using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.ApplicationType;

namespace RegOS.Persistence.Initialization.ReferenceData.Blueprint;

internal static class RegulatoryTemplates
{
    // Deterministic ids from RegulatoryTemplateIds; the authority and
    // submission-type references reuse the ids already seeded elsewhere.
    //
    // Four clinical-trial blueprints (US IND, Canada CTA, Australia CTN,
    // India CT-04). CTD Modules 2–5 are ICH-harmonized, so every blueprint
    // shares them via AddHarmonizedCtdModules; only Module 1 is regional.
    // That split is the whole point — a new authority's dossier is seed data,
    // not code.
    public static IReadOnlyList<RegulatoryTemplate> Data =>
    [
        BuildFdaIndCtd(),
        BuildHcCtaCtd(),
        BuildTgaCtnCtd(),
        BuildCdscoCtaCtd()
    ];

    // ── United States · FDA · IND ────────────────────────────────────────────
    private static RegulatoryTemplate BuildFdaIndCtd()
    {
        var template = RegulatoryTemplate.Create(
            new RegulatoryTemplateId(RegulatoryTemplateIds.FdaIndCtd),
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            new ApplicationTypeId(ApplicationTypeIds.FdaInd),
            "ICH eCTD / FDA");

        // ── v1 — as first published, including its defect ────────────────────
        //
        // This version is WRONG: it places the Investigator's Brochure at 1.13,
        // which FDA's us-regional DTD defines as m1-13-annual-report (evidence
        // E9). It is reproduced here exactly as it shipped, and deprecated
        // below rather than corrected.
        //
        // The seed reproduces history because history has to be reproducible:
        // a freshly cloned database and an upgraded one must contain the same
        // blueprint evolution. Silently fixing v1 here would give two
        // installations two different pasts — and would edit a published
        // version, which the aggregate refuses anyway.
        var v1 = template.StartDraftVersion();
        AddFdaIndModule1AsFirstPublished(template);
        AddHarmonizedCtdModules(template, "FDA-IND");
        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        // ── v2 — corrected against FDA's DTD ─────────────────────────────────
        var v2 = template.StartDraftVersion();
        AddFdaIndModule1Corrected(template);
        AddHarmonizedCtdModules(template, "FDA-IND");
        template.PublishVersion(v2.Id, new DateOnly(2026, 8, 2), DateTime.UtcNow);

        // Existing submissions keep v1; nothing new binds to it.
        template.DeprecateVersion(v1.Id);

        return template;
    }

    /// <summary>
    /// FDA Module 1 as it was first published — <b>with the 1.13 defect</b>.
    /// Do not correct this; correct <see cref="AddFdaIndModule1Corrected"/>.
    /// </summary>
    private static void AddFdaIndModule1AsFirstPublished(RegulatoryTemplate template)
    {
        var m1 = template.AddSection(
            "M1", "Administrative Information and Prescribing Information", null, 1);
        var forms = template.AddSection("1.1", "Forms", m1.Id, 1);
        var coverLetter = template.AddSection("1.2", "Cover Letter", m1.Id, 2);
        template.AddSection("1.3", "Administrative Information", m1.Id, 3);
        template.AddSection("1.4", "References", m1.Id, 4);
        var ib = template.AddSection("1.13", "Investigator's Brochure", m1.Id, 5);
        template.AddSection("1.14", "Labeling", m1.Id, 6);

        AddFdaIndModule1Documents(template, forms, coverLetter, ib);
    }

    /// <summary>
    /// FDA Module 1, corrected against
    /// <c>docs/evidence/EPIC-007a/spec/us-regional-v3-3.dtd</c> (evidence E9).
    /// </summary>
    /// <remarks>
    /// The DTD's Module 1 tree gives <c>m1-13-annual-report</c> and
    /// <c>m1-14-labeling</c>, and puts the brochure three levels down at
    /// <c>m1-14-4-1-investigational-brochure</c>. So 1.14 was always right;
    /// 1.13 was not; and the brochure needs the two intermediate levels that
    /// give RegOS its first four-deep section.
    /// <para>
    /// This is not a label correction. A submission bound to v1 would render
    /// the brochure into the annual-report node — a wrong package, not a wrong
    /// caption.
    /// </para>
    /// </remarks>
    private static void AddFdaIndModule1Corrected(RegulatoryTemplate template)
    {
        var m1 = template.AddSection(
            "M1", "Administrative Information and Prescribing Information", null, 1);
        var forms = template.AddSection("1.1", "Forms", m1.Id, 1);
        var coverLetter = template.AddSection("1.2", "Cover Letter", m1.Id, 2);
        template.AddSection("1.3", "Administrative Information", m1.Id, 3);
        template.AddSection("1.4", "References", m1.Id, 4);

        // m1-13-annual-report — what 1.13 actually is.
        template.AddSection("1.13", "Annual Report", m1.Id, 5);

        // m1-14-labeling → m1-14-4-investigational-drug-labeling
        //               → m1-14-4-1-investigational-brochure
        var labeling = template.AddSection("1.14", "Labeling", m1.Id, 6);
        var investigationalLabeling = template.AddSection(
            "1.14.4", "Investigational Drug Labeling", labeling.Id, 4);
        var ib = template.AddSection(
            "1.14.4.1", "Investigator's Brochure", investigationalLabeling.Id, 1);

        AddFdaIndModule1Documents(template, forms, coverLetter, ib);
    }

    /// <summary>
    /// The documents and rules Module 1 expects. Identical across versions —
    /// only <b>where</b> the brochure belongs changed, never <b>that</b> it is
    /// required.
    /// </summary>
    private static void AddFdaIndModule1Documents(
        RegulatoryTemplate template,
        TemplateSection forms,
        TemplateSection coverLetter,
        TemplateSection investigatorsBrochure)
    {
        template.AddRequiredDocument(
            coverLetter.Id, new DocumentTypeId(DocumentTypeIds.CoverLetter), true, 1);
        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.FormFda1571), true, 1);
        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.FormFda1572), true, 2);
        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.FormFda3674), true, 3);
        template.AddRequiredDocument(
            investigatorsBrochure.Id,
            new DocumentTypeId(DocumentTypeIds.InvestigatorsBrochure), true, 1);

        template.AddValidationRule(
            "FDA-IND-1.1-FORMS-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Error,
            "Module 1.1 (Forms) must contain the required IND forms.",
            sectionId: forms.Id,
            parameters: null,
            order: 2);
    }

    // ── Canada · Health Canada · CTA ─────────────────────────────────────────
    private static RegulatoryTemplate BuildHcCtaCtd()
    {
        var template = RegulatoryTemplate.Create(
            new RegulatoryTemplateId(RegulatoryTemplateIds.HcCtaCtd),
            "HC_CTA_CTD",
            "Health Canada CTA (CTD)",
            new AuthorityId(GeographyAndRegulatoryIds.HealthCanada),
            new ApplicationTypeId(ApplicationTypeIds.HcCta),
            "ICH eCTD / Health Canada");

        var v1 = template.StartDraftVersion();

        // Regional Module 1 (Health Canada) — representative CTA essentials.
        var m1 = template.AddSection(
            "M1", "Administrative and Regional Information (Health Canada)", null, 1);
        var forms = template.AddSection("1.1", "Forms", m1.Id, 1);
        var coverLetter = template.AddSection("1.2", "Cover Letter", m1.Id, 2);
        template.AddSection("1.3", "Administrative Information", m1.Id, 3);
        var ib = template.AddSection("1.4", "Investigator's Brochure", m1.Id, 4);

        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.HcCtaForm), true, 1);
        template.AddRequiredDocument(
            coverLetter.Id, new DocumentTypeId(DocumentTypeIds.CoverLetter), true, 1);
        template.AddRequiredDocument(
            ib.Id, new DocumentTypeId(DocumentTypeIds.InvestigatorsBrochure), true, 1);

        template.AddValidationRule(
            "HC-CTA-1.1-FORMS-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Error,
            "Module 1.1 (Forms) must contain the Health Canada CTA application form.",
            sectionId: forms.Id,
            parameters: null,
            order: 2);

        AddHarmonizedCtdModules(template, "HC-CTA");

        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        return template;
    }

    // ── Australia · TGA · CTN ────────────────────────────────────────────────
    private static RegulatoryTemplate BuildTgaCtnCtd()
    {
        var template = RegulatoryTemplate.Create(
            new RegulatoryTemplateId(RegulatoryTemplateIds.TgaCtnCtd),
            "TGA_CTN_CTD",
            "TGA CTN (CTD)",
            new AuthorityId(GeographyAndRegulatoryIds.TGA),
            new ApplicationTypeId(ApplicationTypeIds.TgaCtn),
            "ICH eCTD / TGA");

        var v1 = template.StartDraftVersion();

        // Regional Module 1 (TGA) — the CTN notification is form-and-protocol led.
        var m1 = template.AddSection(
            "M1", "Administrative and Regional Information (TGA)", null, 1);
        var forms = template.AddSection("1.1", "Forms", m1.Id, 1);
        var coverLetter = template.AddSection("1.2", "Cover Letter", m1.Id, 2);
        var ib = template.AddSection("1.3", "Investigator's Brochure", m1.Id, 3);
        var protocol = template.AddSection("1.4", "Protocol", m1.Id, 4);

        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.TgaCtnForm), true, 1);
        template.AddRequiredDocument(
            coverLetter.Id, new DocumentTypeId(DocumentTypeIds.CoverLetter), true, 1);
        template.AddRequiredDocument(
            ib.Id, new DocumentTypeId(DocumentTypeIds.InvestigatorsBrochure), true, 1);
        template.AddRequiredDocument(
            protocol.Id, new DocumentTypeId(DocumentTypeIds.StudyProtocol), true, 1);

        template.AddValidationRule(
            "TGA-CTN-1.1-FORMS-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Error,
            "Module 1.1 (Forms) must contain the TGA CTN notification form.",
            sectionId: forms.Id,
            parameters: null,
            order: 2);

        AddHarmonizedCtdModules(template, "TGA-CTN");

        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        return template;
    }

    // ── India · CDSCO · Clinical Trial Permission (Form CT-04) ────────────────
    private static RegulatoryTemplate BuildCdscoCtaCtd()
    {
        var template = RegulatoryTemplate.Create(
            new RegulatoryTemplateId(RegulatoryTemplateIds.CdscoCtaCtd),
            "CDSCO_CTA_CTD",
            "CDSCO CTA (CTD)",
            new AuthorityId(GeographyAndRegulatoryIds.CDSCO),
            new ApplicationTypeId(ApplicationTypeIds.CdscoCta),
            "CDSCO / NDCT Rules 2019");

        var v1 = template.StartDraftVersion();

        // Regional Module 1 (CDSCO) — representative, per the NDCT Rules 2019.
        var m1 = template.AddSection(
            "M1", "Administrative and Regional Information (CDSCO)", null, 1);
        var forms = template.AddSection("1.1", "Forms", m1.Id, 1);
        var coverLetter = template.AddSection("1.2", "Cover Letter", m1.Id, 2);
        var ib = template.AddSection("1.3", "Investigator's Brochure", m1.Id, 3);
        var protocol = template.AddSection("1.4", "Protocol", m1.Id, 4);

        template.AddRequiredDocument(
            forms.Id, new DocumentTypeId(DocumentTypeIds.CdscoFormCt04), true, 1);
        template.AddRequiredDocument(
            coverLetter.Id, new DocumentTypeId(DocumentTypeIds.CoverLetter), true, 1);
        template.AddRequiredDocument(
            ib.Id, new DocumentTypeId(DocumentTypeIds.InvestigatorsBrochure), true, 1);
        template.AddRequiredDocument(
            protocol.Id, new DocumentTypeId(DocumentTypeIds.StudyProtocol), true, 1);

        template.AddValidationRule(
            "CDSCO-CTA-1.1-FORMS-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Error,
            "Module 1.1 (Forms) must contain Form CT-04.",
            sectionId: forms.Id,
            parameters: null,
            order: 2);

        AddHarmonizedCtdModules(template, "CDSCO-CTA");

        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        return template;
    }

    /// <summary>
    /// Adds the ICH-harmonized CTD Modules 2–5 — identical across authorities —
    /// to a template's open draft: the section families, the documents they
    /// expect, and the format/stability rules. <paramref name="rulePrefix"/>
    /// namespaces the rule codes per blueprint (rule codes are unique within a
    /// version). Module 1 is regional and stays with each caller.
    /// </summary>
    private static void AddHarmonizedCtdModules(
        RegulatoryTemplate template, string rulePrefix)
    {
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

        // ── Harmonized required documents ────────────────────────────────────
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

        // ── Harmonized validation rules ──────────────────────────────────────
        template.AddValidationRule(
            $"{rulePrefix}-PDF",
            ValidationRuleType.FileFormat,
            ValidationSeverity.Error,
            "All submission documents must be provided as PDF.",
            sectionId: null,
            parameters: "pdf",
            order: 1);
        template.AddValidationRule(
            $"{rulePrefix}-3.2.S.7-STABILITY-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Warning,
            "Drug Substance stability data (3.2.S.7) is expected.",
            sectionId: sStability.Id,
            parameters: null,
            order: 3);
        template.AddValidationRule(
            $"{rulePrefix}-3.2.P.8-STABILITY-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Warning,
            "Drug Product stability data (3.2.P.8) is expected.",
            sectionId: pStability.Id,
            parameters: null,
            order: 4);
    }
}
