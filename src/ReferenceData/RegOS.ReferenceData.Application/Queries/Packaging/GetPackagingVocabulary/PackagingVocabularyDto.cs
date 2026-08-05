using RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;

namespace RegOS.ReferenceData.Application.Queries.Packaging.GetPackagingVocabulary;

/// <param name="Materials">
/// <b>The attribute that makes a package item not a component</b> (ADR-061 §1).
/// Optional on a layer: an outer carton's board grade is rarely stated, while a
/// blister's laminate always is.
/// </param>
/// <remarks>
/// Its own payload rather than fields on the pharmaceutical vocabulary: that one
/// answers <em>what is this medicine?</em> and this answers <em>how is it
/// held?</em>. Offering "blister" beside "tablet" in one list is the first step
/// towards a pack stating what the presentation already says.
/// <para>
/// Units of presentation are deliberately <b>absent</b> — a layer's quantity
/// counts the same units a presentation does, and the pack form already loads
/// that vocabulary. A second copy would be two lists to keep in step.
/// </para>
/// </remarks>
public sealed record PackagingVocabularyDto(
    IReadOnlyList<CodedConceptDto> PackageItemTypes,
    IReadOnlyList<CodedConceptDto> Materials);
