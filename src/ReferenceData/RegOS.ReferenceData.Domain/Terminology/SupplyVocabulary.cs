namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The words that say how a pack may be handed over, and how it must be kept.
/// </summary>
/// <remarks>
/// <b>The fourth vocabulary, and each now answers one question.</b>
/// <see cref="PharmaceuticalVocabulary"/> — <em>what is this medicine?</em>
/// <see cref="MeasurementVocabulary"/> — <em>how much?</em>
/// <see cref="PackagingVocabulary"/> — <em>how is it held?</em> This one —
/// <em>how may it be supplied, and how must it be stored?</em>
/// <para>
/// <b><see cref="ShelfLifePeriods"/> is deliberately not part of
/// <see cref="MeasurementVocabulary"/></b>, and that vocabulary's own reason for
/// existing is the argument. It is kept apart from the presentation's units so
/// that <em>"500 mg per tablet"</em> cannot be expressed; putting
/// <c>MONTH</c> beside <c>MG</c> would make <b>"500 months"</b> a legal
/// strength. Mechanical reuse, semantic nonsense.
/// </para>
/// <para>
/// <b>EDQM's and the EU's concepts, not their terms.</b> Legal status of supply
/// and storage conditions are both controlled terminology in the real world;
/// RegOS holds no such licence, so every entry is
/// <see cref="CodingSystems.RegosInternal"/> and says so (ADR-058 §6).
/// </para>
/// </remarks>
public static class SupplyVocabulary
{
    /// <summary>
    /// The one storage condition that excludes every other. The rule itself
    /// lives with the value object that holds the conditions, because it is a
    /// statement about a set rather than about a term.
    /// </summary>
    public const string NoSpecialPrecautionsCode = "NO_SPECIAL_PRECAUTIONS";

    /// <summary>
    /// Who may hand this pack over, and on what authority.
    /// </summary>
    /// <remarks>
    /// <b>Recorded per pack, not per product</b>
    /// (<see href="../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>
    /// §1's discriminator): a 16-tablet pack of paracetamol may be general sale
    /// where a 100-tablet pack of the same tablets is pharmacy-only. The
    /// restriction follows the quantity supplied, not the active substance.
    /// <para>
    /// Six entries covering the shapes the EU, the UK and the US all have, and
    /// deliberately none of their exact lists — a jurisdiction's own
    /// classification arrives with its terminology, as a data migration.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<CodedConcept> LegalStatuses { get; } =
    [
        CodedConcept.Internal("PRESCRIPTION_ONLY", "Prescription only"),
        CodedConcept.Internal(
            "RESTRICTED_PRESCRIPTION", "Restricted medical prescription"),
        CodedConcept.Internal(
            "SPECIAL_PRESCRIPTION", "Special medical prescription"),
        CodedConcept.Internal("HOSPITAL_ONLY", "Hospital use only"),
        CodedConcept.Internal("PHARMACY_ONLY", "Pharmacy only"),
        CodedConcept.Internal("GENERAL_SALE", "General sale"),
    ];

    /// <summary>
    /// How the pack must be kept for its shelf life to hold.
    /// </summary>
    /// <remarks>
    /// <b>Several may apply at once</b> — <em>"Do not store above 25 °C.
    /// Protect from light."</em> is one ordinary statement, which is why this is
    /// a collection rather than a column.
    /// <para>
    /// <b><see cref="NoSpecialPrecautionsCode"/> is a statement, not an
    /// absence.</b> An SmPC that says <em>"This medicinal product does not
    /// require any special storage conditions"</em> has been checked; an empty
    /// list means nobody has said yet. <c>ShelfLifeStorage</c> refuses to hold
    /// it beside another condition, so the two can never blur.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<CodedConcept> StorageConditions { get; } =
    [
        CodedConcept.Internal("BELOW_25", "Do not store above 25 °C"),
        CodedConcept.Internal("BELOW_30", "Do not store above 30 °C"),
        CodedConcept.Internal(
            "REFRIGERATE", "Store in a refrigerator (2 °C – 8 °C)"),
        CodedConcept.Internal("FREEZE", "Store in a freezer"),
        CodedConcept.Internal("DO_NOT_FREEZE", "Do not freeze"),
        CodedConcept.Internal("DO_NOT_REFRIGERATE", "Do not refrigerate"),
        CodedConcept.Internal("PROTECT_FROM_LIGHT", "Protect from light"),
        CodedConcept.Internal("PROTECT_FROM_MOISTURE", "Protect from moisture"),
        CodedConcept.Internal("ORIGINAL_PACKAGE", "Store in the original package"),
        CodedConcept.Internal(
            NoSpecialPrecautionsCode, "No special storage precautions"),
    ];

    /// <summary>
    /// The period a shelf life is expressed in.
    /// </summary>
    /// <remarks>
    /// <b>Kept literal, never normalised.</b> <em>"3 years"</em> is stored as
    /// <c>3</c> and <c>YEAR</c>, not as <c>36</c> and <c>MONTH</c>. Converting
    /// would be the first unit arithmetic in RegOS, against the position
    /// <c>Strength</c> already records — and a shelf life is quoted back on a
    /// label in the words it was approved in.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> ShelfLifePeriods { get; } =
    [
        CodedConcept.Internal("HOUR", "hours"),
        CodedConcept.Internal("DAY", "days"),
        CodedConcept.Internal("WEEK", "weeks"),
        CodedConcept.Internal("MONTH", "months"),
        CodedConcept.Internal("YEAR", "years"),
    ];

    public static CodedConcept? LegalStatusOf(string? code)
        => CodedConceptLookup.Find(LegalStatuses, code);

    public static CodedConcept? StorageConditionOf(string? code)
        => CodedConceptLookup.Find(StorageConditions, code);

    public static CodedConcept? ShelfLifePeriodOf(string? code)
        => CodedConceptLookup.Find(ShelfLifePeriods, code);
}
