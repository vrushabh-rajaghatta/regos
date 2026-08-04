using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Tests.Product;

public class AtcCodeTests
{
    [Fact]
    public void AFullCodeIsAccepted()
    {
        AtcCode.Create("N02BE01").Value.Should().Be("N02BE01");
    }

    [Fact]
    public void CaseAndSurroundingSpaceDoNotMakeADifferentCode()
    {
        AtcCode.Create(" n02be01 ").Should().Be(AtcCode.Create("N02BE01"));
    }

    /// <summary>
    /// A class is a real answer. A product's own code is assigned late, and a
    /// tenant who knows only that it is an analgesic should be able to say so.
    /// </summary>
    [Theory]
    [InlineData("N")]
    [InlineData("N02")]
    [InlineData("N02B")]
    [InlineData("N02BE")]
    public void APartialCodeIsAccepted(string code)
    {
        AtcCode.Create(code).Value.Should().Be(code);
    }

    /// <summary>
    /// The reason the shape is worth checking at all: <em>"show me every
    /// analgesic"</em> is a prefix match, not a table of parent codes.
    /// </summary>
    [Fact]
    public void AFullCodeYieldsItsFiveLevels()
    {
        AtcCode.Create("N02BE01").Levels
            .Should().Equal("N", "N02", "N02B", "N02BE", "N02BE01");
    }

    [Fact]
    public void APartialCodeYieldsOnlyTheLevelsItReaches()
    {
        AtcCode.Create("N02B").Levels.Should().Equal("N", "N02", "N02B");
    }

    [Theory]
    [InlineData("N0")]        // stops mid-group
    [InlineData("N02BE0")]    // stops mid-group
    [InlineData("NN2BE01")]   // digit position holds a letter
    [InlineData("102BE01")]   // letter position holds a digit
    [InlineData("N02BE011")]  // too long
    [InlineData("N02-BE01")]  // punctuation
    public void AMalformedCodeIsRefused(string code)
    {
        var act = () => AtcCode.Create(code);

        act.Should().Throw<DomainException>()
            .WithMessage(AtcCodeErrors.Malformed);
    }

    /// <summary>
    /// Absence is ordinary — a market presence exists long before anyone
    /// records a classification — so clearing is not an error.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankMeansNoClassification(string? value)
    {
        AtcCode.CreateOrNull(value).Should().BeNull();
    }

    [Fact]
    public void CreateStillRefusesBlank()
    {
        var act = () => AtcCode.Create(null);

        act.Should().Throw<DomainException>()
            .WithMessage(AtcCodeErrors.Required);
    }

    /// <summary>
    /// The claim this type is entitled to make is narrow: the tenant supplied
    /// it. The refusal says so, so acceptance is never read as verification.
    /// </summary>
    [Fact]
    public void TheRefusalSaysRegOSIsNotCheckingMembership()
    {
        AtcCodeErrors.Malformed.Should().Contain("does not hold the WHO ATC index");
    }
}
