using FluentAssertions;

using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Domain.Tests.Terminology;

public class MeasurementVocabularyTests
{
    [Fact]
    public void EveryUnitIsDeclaredAsRegosOwn()
    {
        MeasurementVocabulary.Units.Should().OnlyContain(x => x.IsInternal);
    }

    [Fact]
    public void UnitCodesAreDistinct()
    {
        MeasurementVocabulary.Units.Select(x => x.Code)
            .Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// The separation that keeps a strength orthogonal to its presentation.
    /// If an article ever appeared here, <em>"500 mg per tablet"</em> would
    /// become expressible — and would state what the presentation already
    /// says, in a second place that can disagree.
    /// </summary>
    [Fact]
    public void NoUnitOfPresentationLeaksIntoTheMeasurementVocabulary()
    {
        var articles = PharmaceuticalVocabulary.UnitsOfPresentation
            .Select(x => x.Code);

        MeasurementVocabulary.Units.Select(x => x.Code)
            .Should().NotIntersectWith(articles);
    }

    /// <summary>
    /// Biologicals are expressed in activity units, so a strength cannot assume
    /// it is measuring mass.
    /// </summary>
    [Fact]
    public void ActivityUnitsAreAvailable()
    {
        MeasurementVocabulary.UnitOf("IU").Should().NotBeNull();
    }

    [Fact]
    public void CodeMatchingIgnoresCaseAndSurroundingSpace()
    {
        MeasurementVocabulary.UnitOf(" mg ")!.Code.Should().Be("MG");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("FURLONG")]
    public void AnUnknownUnitResolvesToNothing(string? code)
    {
        MeasurementVocabulary.UnitOf(code).Should().BeNull();
    }

    /// <summary>
    /// Equal in value, never the same object — the EF owned-entity trap, now
    /// guarded once in <c>CodedConceptLookup</c> rather than three times.
    /// </summary>
    [Fact]
    public void EachResolutionIsItsOwnInstance()
    {
        var first = MeasurementVocabulary.UnitOf("MG");
        var second = MeasurementVocabulary.UnitOf("MG");

        first.Should().Be(second);
        first.Should().NotBeSameAs(second);
        first.Should().NotBeSameAs(MeasurementVocabulary.Units[1]);
    }

    [Fact]
    public void TheRefusalListsTheAcceptedCodes()
    {
        MeasurementErrors.UnknownUnit("FURLONG")
            .Should().Contain("FURLONG").And.Contain("MG");
    }
}
