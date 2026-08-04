using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Services;

/// <summary>
/// Turns the codes on the wire into the coded values a pack takes.
/// </summary>
/// <remarks>
/// <b>The same list a presentation and a component already use</b>, not a fourth
/// copy: a pack of 30 tablets counts the same unit a component measures itself
/// in. Shared by add and restate because they take the same fields and must
/// refuse them identically — the reasoning <c>ComponentVocabulary</c> states.
/// </remarks>
internal static class PackVocabulary
{
    public static CodedConcept PackageItemType(string? code)
        => PackagingVocabulary.PackageItemTypeOf(code)
            ?? throw new DomainException(
                PackagingVocabularyErrors.UnknownPackageItemType(code));

    public static CodedConcept? Material(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : PackagingVocabulary.MaterialOf(code)
                ?? throw new DomainException(
                    PackagingVocabularyErrors.UnknownMaterial(code));

    public static CodedConcept? UnitOfPresentation(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : PharmaceuticalVocabulary.UnitOfPresentationOf(code)
                ?? throw new DomainException(
                    PharmaceuticalVocabularyErrors.UnknownUnitOfPresentation(code));
}
