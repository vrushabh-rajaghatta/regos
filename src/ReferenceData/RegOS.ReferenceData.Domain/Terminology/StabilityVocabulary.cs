namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The words that say what condition a shelf life was demonstrated under, and
/// what condition a market accepts one from.
/// </summary>
/// <remarks>
/// <b>The ninth vocabulary, and its own class because two contexts read it to
/// answer two different questions.</b> Geography asks <em>"which long-term
/// conditions does this market accept?"</em>; Product asks <em>"which was this
/// pack's shelf life established at?"</em> Filing the list under
/// <see cref="GeographyVocabulary"/> would misname it for the pack, and under
/// <see cref="SupplyVocabulary"/> — <em>how may it be supplied, and how must it
/// be stored?</em> — would misname it for the market. Neither owns it, so it
/// stands alone.
/// <para>
/// <b>Conditions, not climatic zones — and that is the whole design.</b> WHO
/// publishes a table of the long-term testing condition each member state
/// accepts; it does <em>not</em> publish a zone letter per country, and ICH
/// withdrew Q1F, which was the guideline zone letters came from. India accepts
/// <b>30 °C/70% RH</b>, which is neither Zone IVA (30/65) nor Zone IVB (30/75) —
/// so a stored <c>Zone = IVB</c> would not be WHO's data but RegOS's
/// interpretation of it, and there would be nothing to check it against
/// (<see href="../../../../docs/evidence/EPIC-022/stability-conditions.md">E39</see>).
/// <em>"Zone IVB"</em> remains a perfectly good thing for a person to say; it is
/// display vocabulary, and it is not persisted (EPIC-022 D6).
/// </para>
/// <para>
/// <b>Nobody publishes the set of conditions either.</b> WHO's table names one
/// per country, the way each regional grouping names only its own membership —
/// so this list, like <see cref="GeographyVocabulary.Regions"/>, is RegOS's own
/// choice of which are worth recording, and it says so
/// (<see cref="CodingSystems.RegosInternal"/>, ADR-058 §6).
/// </para>
/// </remarks>
public static class StabilityVocabulary
{
    /// <summary>
    /// The long-term stability testing conditions RegOS records.
    /// </summary>
    /// <remarks>
    /// <b>Four, and one of them is not accepted by any market RegOS seeds.</b>
    /// <c>30C_75RH</c> is what a global stability programme routinely generates
    /// data at to support hot and humid markets, so a <em>pack</em> needs to be
    /// able to say it even though no <em>country</em> in the eight asks for it.
    /// The vocabulary models the regulated world, not today's seed — which is
    /// the opposite of the reasoning that keeps a speculative field out of an
    /// aggregate, and the difference is that a term nobody uses costs a row in
    /// a list where a field nobody writes costs a lie in the schema.
    /// <para>
    /// The four are the conditions WHO's table names for the eight seeded
    /// markets, plus that one. Its table names others — 30 °C/35% RH,
    /// 30 °C/80% RH — and adding a market that accepts one is a data change to
    /// this list, not a model change.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<CodedConcept> Conditions { get; } =
    [
        CodedConcept.Internal("25C_60RH", "25 °C / 60% RH"),
        CodedConcept.Internal("30C_65RH", "30 °C / 65% RH"),
        CodedConcept.Internal("30C_70RH", "30 °C / 70% RH"),
        CodedConcept.Internal("30C_75RH", "30 °C / 75% RH"),
    ];

    public static CodedConcept? ConditionOf(string? code)
        => CodedConceptLookup.Find(Conditions, code);
}
