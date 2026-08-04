using FluentAssertions;

using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Domain.Tests.Terminology;

public class PharmaceuticalVocabularyTests
{
    /// <summary>
    /// Dose form, route and unit of presentation are all EDQM Standard Terms in
    /// the real world, and RegOS does not hold that licence. Every entry says
    /// it is RegOS's own (ADR-058 §6).
    /// </summary>
    [Fact]
    public void EveryTermIsDeclaredAsRegosOwn()
    {
        PharmaceuticalVocabulary.DoseForms
            .Concat(PharmaceuticalVocabulary.RoutesOfAdministration)
            .Concat(PharmaceuticalVocabulary.UnitsOfPresentation)
            .Should().OnlyContain(x => x.IsInternal);
    }

    [Fact]
    public void DoseFormCodesAreDistinct()
    {
        PharmaceuticalVocabulary.DoseForms.Select(x => x.Code)
            .Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void RouteCodesAreDistinct()
    {
        PharmaceuticalVocabulary.RoutesOfAdministration.Select(x => x.Code)
            .Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void UnitOfPresentationCodesAreDistinct()
    {
        PharmaceuticalVocabulary.UnitsOfPresentation.Select(x => x.Code)
            .Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// The two vocabularies are kept apart on purpose: this one counts
    /// articles, and mg/mL/IU measure quantity and arrive with
    /// <c>Strength</c> in S003. One picker offering both would be the drift
    /// this asserts against.
    /// </summary>
    [Fact]
    public void UnitsOfPresentationHoldNoMeasurementUnits()
    {
        string[] measurements = ["MG", "ML", "IU", "G", "MCG", "L"];

        PharmaceuticalVocabulary.UnitsOfPresentation
            .Select(x => x.Code)
            .Should().NotIntersectWith(measurements);
    }

    [Fact]
    public void AKnownDoseFormResolves()
    {
        PharmaceuticalVocabulary.DoseFormOf("TABLET")!.Display
            .Should().Be("Tablet");
    }

    [Fact]
    public void CodeMatchingIgnoresCaseAndSurroundingSpace()
    {
        PharmaceuticalVocabulary.RouteOf(" oral ")!.Code.Should().Be("ORAL");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("NOT_A_FORM")]
    public void AnUnknownDoseFormResolvesToNothing(string? code)
    {
        PharmaceuticalVocabulary.DoseFormOf(code).Should().BeNull();
    }

    /// <summary>
    /// Equal in value, never the same object.
    /// </summary>
    /// <remarks>
    /// A resolved concept is persisted as an owned entity, and EF tracks one
    /// against exactly one owner — handing out the catalogued instance makes
    /// the second entity to use it look like it has no value at all. That is
    /// how S001's seed failed the first time it ran, and this vocabulary keeps
    /// its own copy of the guard because ADR-018 abstracts on the third
    /// occurrence, not the second.
    /// </remarks>
    [Fact]
    public void EachResolutionIsItsOwnInstance()
    {
        var first = PharmaceuticalVocabulary.DoseFormOf("TABLET");
        var second = PharmaceuticalVocabulary.DoseFormOf("TABLET");

        first.Should().Be(second);
        first.Should().NotBeSameAs(second);
        first.Should().NotBeSameAs(PharmaceuticalVocabulary.DoseForms[0]);
    }

    [Fact]
    public void TheRefusalListsTheAcceptedCodes()
    {
        PharmaceuticalVocabularyErrors.UnknownRoute("SUBLINGUAL")
            .Should().Contain("SUBLINGUAL").And.Contain("ORAL");
    }
}
