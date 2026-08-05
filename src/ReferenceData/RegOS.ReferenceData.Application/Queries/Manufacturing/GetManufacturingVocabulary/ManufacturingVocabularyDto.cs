using RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;

namespace RegOS.ReferenceData.Application.Queries.Manufacturing.GetManufacturingVocabulary;

/// <param name="Operations">
/// What a site may do for a product — manufacture the active substance or the
/// finished product, package it primarily or secondarily, test it, release it,
/// import it.
/// <para>
/// <b>Its own payload rather than a fourth list on the supply vocabulary.</b>
/// That one answers <em>how may this pack be handed over, and how must it be
/// kept?</em>; this answers <em>who does the work?</em>, and no form states
/// both.
/// </para>
/// </param>
public sealed record ManufacturingVocabularyDto(
    IReadOnlyList<CodedConceptDto> Operations);
