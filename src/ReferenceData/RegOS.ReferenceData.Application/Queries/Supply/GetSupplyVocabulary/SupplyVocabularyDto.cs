using RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;

namespace RegOS.ReferenceData.Application.Queries.Supply.GetSupplyVocabulary;

/// <param name="LegalStatuses">
/// Who may hand the pack over. Recorded per pack, not per product: a 16-tablet
/// pack of paracetamol may be general sale where a 100-tablet pack is
/// pharmacy-only (ADR-061 §1).
/// </param>
/// <param name="StorageConditions">
/// Several ordinarily apply at once, which is why the form offers them as a
/// set. <c>NO_SPECIAL_PRECAUTIONS</c> is among them and cannot be combined with
/// another — the value object refuses it, and the form should too.
/// </param>
/// <param name="ShelfLifePeriods">
/// <b>Not the measurement units.</b> A duration is not a quantity, and offering
/// months beside milligrams is how "500 months" becomes a legal strength.
/// </param>
/// <param name="StabilityConditions">
/// What a shelf life may be demonstrated under — <em>25 °C/60% RH</em>.
/// <para>
/// <b>These are not part of <c>SupplyVocabulary</c>, and the code says so</b>:
/// they are <c>StabilityVocabulary</c>, its own class because Geography reads
/// the same list to say which conditions a market accepts. They ride on this
/// payload because they are stated on the same form as the shelf life they
/// qualify, and a second round trip for four entries would be the worse
/// trade. <b>The boundary is in the domain, not in the envelope.</b>
/// </para>
/// </param>
public sealed record SupplyVocabularyDto(
    IReadOnlyList<CodedConceptDto> LegalStatuses,
    IReadOnlyList<CodedConceptDto> StorageConditions,
    IReadOnlyList<CodedConceptDto> ShelfLifePeriods,
    IReadOnlyList<CodedConceptDto> StabilityConditions);
