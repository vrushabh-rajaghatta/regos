namespace RegOS.Submission.Application.StudyTagging;

/// <summary>
/// ICH's controlled vocabulary for <c>file-tag</c> — what role a document plays
/// in a study report.
/// </summary>
/// <remarks>
/// <b>Held as a table here rather than seeded as reference data</b>, because it
/// is a wire vocabulary and not 97 business concepts.
/// <c>data-tabulation-dataset-sdtm</c> and <c>HF-validation-protocol</c> do not
/// name anything that would exist if ICH did not, so
/// <see href="ADR-055">ADR-055</see>'s promotion test fails for the list as a
/// whole — the same call <c>TelephoneNumberTypes</c> and
/// <c>ApplicantContactTypes</c> already make. Nobody curates it; it changes when
/// ICH publishes a new <c>valid-values.xml</c>, and
/// <c>FileTagVocabularyTests</c> is what notices.
/// <para>
/// <b>Transcribed by parsing the held file, never by hand</b> (E33). The
/// realm — <c>info-type</c> on the wire — is a <em>value</em> here rather than a
/// second column on the placement, because all 97 values are distinct across
/// <c>ich</c>, <c>us</c> and <c>jp</c>: the realm is a function of the name, so
/// storing both would be storing a fact that can disagree with itself. The test
/// asserts that uniqueness, which is what keeps the derivation honest.
/// </para>
/// <para>
/// <b>It does not say which tag belongs on which document.</b> RegOS has the
/// words and can refuse a non-word; choosing between <c>synopsis</c> and
/// <c>study-report-body</c> is the filer's judgement, and the guidance that
/// would narrow it is not held.
/// </para>
/// </remarks>
public static class FileTagVocabulary
{
    /// <summary>
    /// Every published <c>file-tag</c>, mapped to the realm it belongs to.
    /// Ordinal comparison: these are wire tokens, and <c>PK-PD-relationship</c>
    /// is not <c>pk-pd-relationship</c>.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> Realms =
        new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["pre-clinical-study-report"] = "ich",
        ["legacy-clinical-study-report"] = "ich",
        ["synopsis"] = "ich",
        ["study-report-body"] = "ich",
        ["protocol-or-amendment"] = "ich",
        ["sample-case-report-form"] = "ich",
        ["iec-irb-consent-form-list"] = "ich",
        ["list-description-investigator-site"] = "ich",
        ["signatures-investigators"] = "ich",
        ["list-patients-with-batches"] = "ich",
        ["randomisation-scheme"] = "ich",
        ["audit-certificates-report"] = "ich",
        ["statistical-methods-interim-analysis-plan"] = "ich",
        ["inter-laboratory-standardisation-methods-quality-assurance"] = "ich",
        ["publications-based-on-study"] = "ich",
        ["publications-referenced-in-report"] = "ich",
        ["discontinued-patients"] = "ich",
        ["protocol-deviations"] = "ich",
        ["patients-excluded-from-efficacy-analysis"] = "ich",
        ["demographic-data"] = "ich",
        ["compliance-and-drug-concentration-data"] = "ich",
        ["individual-efficacy-response-data"] = "ich",
        ["adverse-event-listings"] = "ich",
        ["listing-individual-laboratory-measurements-by-patient"] = "ich",
        ["case-report-forms"] = "ich",
        ["available-on-request"] = "ich",
        ["assay-validation"] = "ich",
        ["biomarkers"] = "ich",
        ["data-monitoring-review-committees"] = "ich",
        ["device-information"] = "ich",
        ["diagnostic-tests"] = "ich",
        ["gene-therapy"] = "ich",
        ["patient-reported-outcomes"] = "ich",
        ["pharmacodynamics"] = "ich",
        ["pharmacogenomics"] = "ich",
        ["pharmacokinetics"] = "ich",
        ["quality-of-life"] = "ich",
        ["stem-cells"] = "ich",
        ["abuse-liability"] = "ich",
        ["antibody"] = "ich",
        ["healthcare-utilization"] = "ich",
        ["other-data-not-specified"] = "ich",
        ["PK-PD-relationship"] = "ich",
        ["specialty-report"] = "ich",
        ["bimo"] = "ich",
        ["foreign-clinical-studies-not-under-ind"] = "us",
        ["complete-patient-list"] = "jp",
        ["serious-adverse-event-patient-list"] = "jp",
        ["adverse-event-patient-list"] = "jp",
        ["abnormal-lab-values-patient-list"] = "jp",
        ["data-tabulation-dataset-legacy"] = "us",
        ["data-tabulation-dataset-sdtm"] = "us",
        ["data-tabulation-dataset-send"] = "us",
        ["data-tabulation-data-definition"] = "us",
        ["data-listing-dataset"] = "us",
        ["data-listing-data-definition"] = "us",
        ["analysis-dataset-adam"] = "us",
        ["analysis-dataset-legacy"] = "us",
        ["analysis-program"] = "us",
        ["analysis-data-definition"] = "us",
        ["annotated-crf"] = "us",
        ["ecg"] = "us",
        ["image"] = "us",
        ["subject-profiles"] = "us",
        ["safety-report"] = "us",
        ["antibacterial"] = "us",
        ["special-pathogen"] = "us",
        ["antiviral"] = "us",
        ["iss"] = "us",
        ["ise"] = "us",
        ["pm-description"] = "us",
        ["HF-validation-protocol"] = "us",
        ["HF-validation-report"] = "us",
        ["HF-validation-other"] = "us",
        ["csr_other"] = "ich",
        ["hepatic-impairment-study"] = "ich",
        ["renal-impairment-study"] = "ich",
        ["drug-drug-interaction-study"] = "ich",
        ["mass-balance-study"] = "ich",
        ["population-pk-report"] = "ich",
        ["population-pkpd-report"] = "ich",
        ["pbpk-report"] = "ich",
        ["pbbm-report"] = "ich",
        ["qsp-report"] = "ich",
        ["cp-general"] = "ich",
        ["qt-clinical-study"] = "ich",
        ["qt-invitro-study"] = "ich",
        ["pd-invivo-study"] = "ich",
        ["pd-invitro-study"] = "ich",
        ["iscp"] = "ich",
        ["isi"] = "ich",
        ["study-data-reviewers-guide"] = "ich",
        ["analysis-data-reviewers-guide"] = "ich",
        ["weight-of-evidence"] = "ich",
        ["animal-rule-efficacy"] = "ich",
        ["animal-rule-natural-history"] = "ich",
        ["nonstandard-safety-study"] = "ich",
    };

    public static IReadOnlyCollection<string> All => (IReadOnlyCollection<string>)Realms.Keys;

    public static bool Contains(string fileTag) => Realms.ContainsKey(fileTag);

    /// <summary>
    /// The <c>info-type</c> this tag is published under — <c>ich</c>, <c>us</c>
    /// or <c>jp</c>. Emitting the wrong one produces a file the DTD accepts and
    /// the ICH stylesheet paints red (E34), which is why it is looked up rather
    /// than assumed to be <c>ich</c>.
    /// </summary>
    public static string RealmOf(string fileTag) => Realms[fileTag];

    public static IReadOnlyDictionary<string, string> AsMap => Realms;
}
