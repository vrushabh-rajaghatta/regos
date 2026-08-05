using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Services;

/// <summary>
/// Turns the codes on the wire into the coded values the aggregate takes,
/// refusing any word RegOS does not know.
/// </summary>
/// <remarks>
/// <b>"Is this a word?" is answered at the boundary, not by the aggregate</b> —
/// the division EPIC-019 settled when the generator, and not the study, refused
/// an identifier a filename could not carry. It also keeps the vocabulary
/// swappable: when licensed terminology arrives, this resolution changes and
/// the domain does not.
/// <para>
/// Shared by <c>AddPresentation</c> and <c>RestatePresentation</c> because they
/// take the same six fields and must refuse them identically — a presentation
/// that could be restated into a state it could not be created in would be a
/// gap, not a feature.
/// </para>
/// </remarks>
internal static class PresentationVocabulary
{
    public static CodedConcept DoseForm(string? code)
        => PharmaceuticalVocabulary.DoseFormOf(code)
            ?? throw new DomainException(
                PharmaceuticalVocabularyErrors.UnknownDoseForm(code));

    public static CodedConcept? UnitOfPresentation(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : PharmaceuticalVocabulary.UnitOfPresentationOf(code)
                ?? throw new DomainException(
                    PharmaceuticalVocabularyErrors.UnknownUnitOfPresentation(code));

    public static CodedConcept? Shape(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : PharmaceuticalVocabulary.ShapeOf(code)
                ?? throw new DomainException(
                    PharmaceuticalVocabularyErrors.UnknownShape(code));

    /// <remarks>
    /// Every code is resolved before any is applied, so a list with one bad
    /// entry is refused whole rather than half-applied.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> Colours(IReadOnlyList<string>? codes)
        => [.. (codes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => PharmaceuticalVocabulary.ColourOf(code)
                ?? throw new DomainException(
                    PharmaceuticalVocabularyErrors.UnknownColour(code)))];

    public static IReadOnlyList<CodedConcept> Routes(IReadOnlyList<string>? codes)
        => (codes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => PharmaceuticalVocabulary.RouteOf(code)
                ?? throw new DomainException(
                    PharmaceuticalVocabularyErrors.UnknownRoute(code)))
            .ToList();
}
