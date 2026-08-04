namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The words a presentation's dose form, routes and unit of presentation may
/// be drawn from during MVP.
/// </summary>
/// <remarks>
/// <b>Code, not a table</b> — the shape <see cref="SubstanceVocabulary"/> took,
/// and EPIC-019's <c>file-tag</c> before it. A vocabulary nobody stewards and
/// every write validates against is a table that only ever gets read; holding
/// it in code keeps it versioned with the rule that uses it.
/// <para>
/// <b>The third vocabulary arrived, so the lookup was collapsed</b> into
/// <see cref="CodedConceptLookup"/> — <see cref="MeasurementVocabulary"/> was
/// the occurrence ADR-018 was waiting for. The lists themselves stay apart:
/// they answer different questions, not three shapes of one.
/// </para>
/// <para>
/// <b>These are EDQM's concepts and not EDQM's terms.</b> Dose form, route of
/// administration and unit of presentation are all EDQM Standard Terms in the
/// real world; RegOS does not hold that licence, so every entry here is
/// <see cref="CodingSystems.RegosInternal"/> and says so. The codes are ours,
/// deliberately unlike EDQM's numeric ones, so that a value can never be
/// mistaken for a licensed one (ADR-058 §6).
/// </para>
/// </remarks>
public static class PharmaceuticalVocabulary
{
    /// <summary>What the product physically is when administered.</summary>
    public static IReadOnlyList<CodedConcept> DoseForms { get; } =
    [
        CodedConcept.Internal("TABLET", "Tablet"),
        CodedConcept.Internal("FILM_COATED_TABLET", "Film-coated tablet"),
        CodedConcept.Internal("CAPSULE", "Capsule"),
        CodedConcept.Internal("ORAL_SOLUTION", "Oral solution"),
        CodedConcept.Internal("ORAL_SUSPENSION", "Oral suspension"),
        CodedConcept.Internal("SOLUTION_FOR_INJECTION", "Solution for injection"),
        CodedConcept.Internal(
            "POWDER_FOR_SOLUTION_FOR_INJECTION",
            "Powder for solution for injection"),
        CodedConcept.Internal("SOLUTION_FOR_INFUSION", "Solution for infusion"),
        CodedConcept.Internal("CREAM", "Cream"),
        CodedConcept.Internal("OINTMENT", "Ointment"),
        CodedConcept.Internal("EYE_DROPS", "Eye drops"),
        CodedConcept.Internal("INHALATION_POWDER", "Inhalation powder"),
        CodedConcept.Internal("SUPPOSITORY", "Suppository"),
        CodedConcept.Internal("TRANSDERMAL_PATCH", "Transdermal patch")
    ];

    /// <summary>How it enters the body.</summary>
    public static IReadOnlyList<CodedConcept> RoutesOfAdministration { get; } =
    [
        CodedConcept.Internal("ORAL", "Oral"),
        CodedConcept.Internal("INTRAVENOUS", "Intravenous"),
        CodedConcept.Internal("INTRAMUSCULAR", "Intramuscular"),
        CodedConcept.Internal("SUBCUTANEOUS", "Subcutaneous"),
        CodedConcept.Internal("TOPICAL", "Topical"),
        CodedConcept.Internal("INHALATION", "Inhalation"),
        CodedConcept.Internal("OPHTHALMIC", "Ophthalmic"),
        CodedConcept.Internal("RECTAL", "Rectal"),
        CodedConcept.Internal("TRANSDERMAL", "Transdermal"),
        CodedConcept.Internal("NASAL", "Nasal")
    ];

    /// <summary>
    /// The countable thing a patient receives — a tablet, a vial, an ampoule.
    /// </summary>
    /// <remarks>
    /// <b>Not a strength unit.</b> This vocabulary counts articles; mg, mL and
    /// IU measure quantity and arrive with <c>Strength</c> in S003. Keeping
    /// them apart is what stops "one vial" and "5 mL" being offered in the same
    /// picker.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> UnitsOfPresentation { get; } =
    [
        CodedConcept.Internal("TABLET", "Tablet"),
        CodedConcept.Internal("CAPSULE", "Capsule"),
        CodedConcept.Internal("VIAL", "Vial"),
        CodedConcept.Internal("AMPOULE", "Ampoule"),
        CodedConcept.Internal("PRE_FILLED_SYRINGE", "Pre-filled syringe"),
        CodedConcept.Internal("PRE_FILLED_PEN", "Pre-filled pen"),
        CodedConcept.Internal("SACHET", "Sachet"),
        CodedConcept.Internal("BOTTLE", "Bottle"),
        CodedConcept.Internal("TUBE", "Tube"),
        CodedConcept.Internal("PATCH", "Patch")
    ];

    public static CodedConcept? DoseFormOf(string? code)
        => CodedConceptLookup.Find(DoseForms, code);

    public static CodedConcept? RouteOf(string? code)
        => CodedConceptLookup.Find(RoutesOfAdministration, code);

    public static CodedConcept? UnitOfPresentationOf(string? code)
        => CodedConceptLookup.Find(UnitsOfPresentation, code);
}
