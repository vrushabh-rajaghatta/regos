using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Services;

/// <summary>
/// Turns the codes on the wire into the coded values a component takes.
/// </summary>
/// <remarks>
/// Shared by add and restate because they take the same fields and must refuse
/// them identically — the same reasoning as <c>PresentationVocabulary</c>.
/// </remarks>
internal static class ComponentVocabulary
{
    public static CodedConcept ComponentType(string? code)
        => PharmaceuticalVocabulary.ComponentTypeOf(code)
            ?? throw new DomainException(
                PharmaceuticalVocabularyErrors.UnknownComponentType(code));

    public static CodedConcept? UnitOfPresentation(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : PharmaceuticalVocabulary.UnitOfPresentationOf(code)
                ?? throw new DomainException(
                    PharmaceuticalVocabularyErrors.UnknownUnitOfPresentation(code));

    public static CodedConcept? DoseForm(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : PharmaceuticalVocabulary.DoseFormOf(code)
                ?? throw new DomainException(
                    PharmaceuticalVocabularyErrors.UnknownDoseForm(code));
}
