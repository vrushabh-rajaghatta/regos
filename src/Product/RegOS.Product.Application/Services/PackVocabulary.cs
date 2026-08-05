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

    /// <remarks>
    /// Null clears the classification, which is a real act: a pack recorded
    /// before its legal status is known has none.
    /// </remarks>
    public static CodedConcept? LegalStatus(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : SupplyVocabulary.LegalStatusOf(code)
                ?? throw new DomainException(
                    SupplyVocabularyErrors.UnknownLegalStatus(code));

    /// <remarks>
    /// <b>Not <see cref="MeasurementVocabulary"/>.</b> A shelf life is a
    /// duration, and putting months beside milligrams would make "500 months" a
    /// legal strength.
    /// </remarks>
    public static CodedConcept? ShelfLifePeriod(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : SupplyVocabulary.ShelfLifePeriodOf(code)
                ?? throw new DomainException(
                    SupplyVocabularyErrors.UnknownShelfLifePeriod(code));

    /// <remarks>
    /// Every code is resolved before any is applied, so a list with one bad
    /// entry is refused whole rather than half-applied. The value object then
    /// rules on the set — duplicates, and "no special precautions" standing
    /// beside a precaution.
    /// </remarks>
    public static IReadOnlyList<CodedConcept> StorageConditions(
        IEnumerable<string>? codes)
        => [.. (codes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => SupplyVocabulary.StorageConditionOf(code)
                ?? throw new DomainException(
                    SupplyVocabularyErrors.UnknownStorageCondition(code)))];

    /// <remarks>
    /// <b>Not <see cref="StorageConditions"/>, and the two lists share no
    /// entry.</b> Those say how the pack must be kept; these say what its shelf
    /// life was demonstrated under, and a market reads the second to decide
    /// whether the period holds there
    /// (<see cref="StabilityVocabulary"/>, its own class because Geography reads
    /// the same list to say what it accepts).
    /// </remarks>
    public static IReadOnlyList<CodedConcept> TestedAt(IEnumerable<string>? codes)
        => [.. (codes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => StabilityVocabulary.ConditionOf(code)
                ?? throw new DomainException(
                    StabilityVocabularyErrors.UnknownCondition(code)))];
}
