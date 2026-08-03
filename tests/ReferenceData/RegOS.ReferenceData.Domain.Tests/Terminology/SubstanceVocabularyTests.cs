using FluentAssertions;

using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Domain.Tests.Terminology;

public class SubstanceVocabularyTests
{
    /// <summary>
    /// The claim RegOS is allowed to make about this vocabulary and no more:
    /// it is ours. A row here that quoted EDQM or GSRS would assert terminology
    /// the platform does not hold — EPIC-019's failure, and what ADR-058 §6 is
    /// written to prevent.
    /// </summary>
    [Fact]
    public void EverySeededTermIsDeclaredAsRegosOwn()
    {
        SubstanceVocabulary.Classes
            .Concat(SubstanceVocabulary.Types)
            .Should().OnlyContain(x => x.IsInternal);
    }

    [Fact]
    public void ClassCodesAreDistinct()
    {
        SubstanceVocabulary.Classes.Select(x => x.Code)
            .Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void TypeCodesAreDistinct()
    {
        SubstanceVocabulary.Types.Select(x => x.Code)
            .Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AKnownClassCodeResolves()
    {
        SubstanceVocabulary.ClassOf("CHEMICAL")!.Display.Should().Be("Chemical");
    }

    /// <summary>
    /// A code arriving from a form or a script should not be refused over
    /// letter casing it never chose.
    /// </summary>
    [Fact]
    public void CodeMatchingIgnoresCaseAndSurroundingSpace()
    {
        SubstanceVocabulary.TypeOf(" synthetic ")!.Code.Should().Be("SYNTHETIC");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("NOT_A_CLASS")]
    public void AnUnknownClassCodeResolvesToNothing(string? code)
    {
        SubstanceVocabulary.ClassOf(code).Should().BeNull();
    }

    /// <summary>
    /// Equal in value, and never the same object.
    /// </summary>
    /// <remarks>
    /// A resolved concept is persisted as an owned entity, and EF tracks one
    /// against exactly one owner. Handing the catalogued instance to two
    /// substances makes the second look like it has no class at all — which is
    /// how the seed failed the first time it ran.
    /// </remarks>
    [Fact]
    public void EachResolutionIsItsOwnInstance()
    {
        var first = SubstanceVocabulary.ClassOf("CHEMICAL");
        var second = SubstanceVocabulary.ClassOf("CHEMICAL");

        first.Should().Be(second);
        first.Should().NotBeSameAs(second);
        first.Should().NotBeSameAs(SubstanceVocabulary.Classes[0]);
    }

    /// <summary>
    /// The refusal names what would have been accepted, so a caller who sent
    /// the wrong code can act on the answer.
    /// </summary>
    [Fact]
    public void TheRefusalListsTheAcceptedCodes()
    {
        SubstanceVocabularyErrors.UnknownClass("POWDER")
            .Should().Contain("POWDER").And.Contain("CHEMICAL");
    }
}
