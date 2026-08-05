using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Services;

/// <summary>
/// Turns the codes on the wire into the coded values a product's manufacturing
/// takes.
/// </summary>
/// <remarks>
/// <b>Its own class rather than a seventh method on <c>PackVocabulary</c></b>,
/// which resolves the words a <em>pack</em> is described by — size, layer
/// material, legal status, shelf-life period, testing condition. Where work
/// happens is not a fact about a pack, and the two lists share no entry.
/// </remarks>
internal static class ProductVocabulary
{
    /// <remarks>
    /// Required rather than nullable: an operation with no type says a site is
    /// involved without saying how, which is not a record anybody can act on.
    /// </remarks>
    public static CodedConcept ManufacturingOperation(string? code)
        => ManufacturingVocabulary.OperationOf(code)
            ?? throw new DomainException(
                ManufacturingVocabularyErrors.UnknownOperation(code));
}
