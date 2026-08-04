namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The words a substance's class and type may be drawn from during MVP.
/// </summary>
/// <remarks>
/// <b>Code, not a table</b> — the shape EPIC-019 settled for <c>file-tag</c>.
/// A vocabulary nobody stewards and every write must validate against is a
/// table that only ever gets read; holding it in code keeps it versioned with
/// the rule that uses it and makes the arrival of licensed terminology a
/// visible change rather than a silent row.
/// <para>
/// <b>The distinction between class and type is ours, and is not evidenced.</b>
/// RIM's substance sheet carries both and states no difference between them
/// (EPIC-010a D7 treats the sheet as defective here). RegOS reads
/// <see cref="Classes"/> as the <em>structural</em> axis — the one ISO 11238
/// organises substances by — and <see cref="Types"/> as the <em>origin</em>
/// axis. That reading is plausible and unsupported by anything RegOS holds; it
/// is recorded here rather than asserted elsewhere, and
/// <see cref="CodingSystems.RegosInternal"/> is what lets licensed terminology
/// correct it as data (ADR-058 §6).
/// </para>
/// </remarks>
public static class SubstanceVocabulary
{
    /// <summary>The structural axis: what kind of thing the substance is.</summary>
    public static IReadOnlyList<CodedConcept> Classes { get; } =
    [
        CodedConcept.Internal("CHEMICAL", "Chemical"),
        CodedConcept.Internal("PROTEIN", "Protein"),
        CodedConcept.Internal("NUCLEIC_ACID", "Nucleic acid"),
        CodedConcept.Internal("POLYMER", "Polymer"),
        CodedConcept.Internal("MIXTURE", "Mixture"),
        CodedConcept.Internal("STRUCTURALLY_DIVERSE", "Structurally diverse")
    ];

    /// <summary>The origin axis: where the substance comes from.</summary>
    public static IReadOnlyList<CodedConcept> Types { get; } =
    [
        CodedConcept.Internal("SYNTHETIC", "Synthetic"),
        CodedConcept.Internal("SEMI_SYNTHETIC", "Semi-synthetic"),
        CodedConcept.Internal("BIOLOGICAL", "Biological"),
        CodedConcept.Internal("HERBAL", "Herbal"),
        CodedConcept.Internal("MINERAL", "Mineral")
    ];

    /// <summary>
    /// The class named by <paramref name="code"/>, or null if RegOS's
    /// vocabulary does not contain it.
    /// </summary>
    public static CodedConcept? ClassOf(string? code)
        => CodedConceptLookup.Find(Classes, code);

    /// <summary>
    /// The type named by <paramref name="code"/>, or null if RegOS's
    /// vocabulary does not contain it.
    /// </summary>
    public static CodedConcept? TypeOf(string? code)
        => CodedConceptLookup.Find(Types, code);
}
