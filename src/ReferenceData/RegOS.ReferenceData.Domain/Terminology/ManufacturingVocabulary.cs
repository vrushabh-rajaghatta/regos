namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The words that say what a site does for a product.
/// </summary>
/// <remarks>
/// <b>The tenth vocabulary, and each still answers one question.</b>
/// <see cref="PharmaceuticalVocabulary"/> — <em>what is this medicine?</em>
/// <see cref="PackagingVocabulary"/> — <em>how is it held?</em>
/// <see cref="SupplyVocabulary"/> — <em>how may it be supplied, and how
/// stored?</em> <see cref="StabilityVocabulary"/> — <em>what was its shelf life
/// demonstrated under?</em> This one — <em>what operation does this site perform
/// on it?</em>
/// <para>
/// <b>Data and not an enum, on the test <c>OrganizationSiteType</c> records for
/// going the other way.</b> That type is a closed enum <em>because rules branch
/// on it</em> — only a manufacturing site may be named on a licence as an
/// approved manufacturer. <b>Nothing branches on an operation type.</b> A
/// business rule that read <c>if (operation.Code == "BATCH_RELEASE")</c> would
/// mean this had stopped being vocabulary and become a closed set, and that is
/// the signal to come back here (EPIC-010c D4).
/// </para>
/// <para>
/// <b>This list is why RegOS has no <c>Manufacturer</c> column</b> on
/// <c>PackagedProduct</c> or <c>PackageItem</c>, where RIM puts one. The
/// distinction those columns were drawing — who packs it, who tests it, who
/// releases it — is carried here, on a single relationship, instead of being
/// spread across three aggregates that could disagree
/// (<see href="../../../../docs/adr/ADR-063-where-a-product-is-made-is-a-product-fact.md">ADR-063</see>
/// §3).
/// </para>
/// </remarks>
public static class ManufacturingVocabulary
{
    /// <summary>
    /// The operations a site may perform for a product.
    /// </summary>
    /// <remarks>
    /// <b>Seven, chosen because each is separately authorised in the real
    /// world.</b> A marketing authorisation names its finished-product site, its
    /// batch-release site and its testing site as distinct entries, and they are
    /// routinely different companies in different countries — which is the whole
    /// reason this is a list rather than a flag.
    /// <para>
    /// <b>The primary/secondary packaging split is not cosmetic.</b> Primary
    /// packaging touches the product and belongs to the sterile boundary;
    /// secondary packaging is the carton and the leaflet, and is frequently done
    /// locally per market. A single <em>"packaging"</em> term would make a
    /// local repackager indistinguishable from the blister line.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<CodedConcept> Operations { get; } =
    [
        CodedConcept.Internal(
            "API_MANUFACTURE", "Manufacture of active substance"),
        CodedConcept.Internal(
            "FINISHED_PRODUCT", "Manufacture of finished product"),
        CodedConcept.Internal("PRIMARY_PACKAGING", "Primary packaging"),
        CodedConcept.Internal("SECONDARY_PACKAGING", "Secondary packaging"),
        CodedConcept.Internal("QC_TESTING", "Quality control testing"),
        CodedConcept.Internal("BATCH_RELEASE", "Batch release"),
        CodedConcept.Internal("IMPORTATION", "Importation"),
    ];

    public static CodedConcept? OperationOf(string? code)
        => CodedConceptLookup.Find(Operations, code);
}
