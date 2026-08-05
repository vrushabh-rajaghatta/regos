using FluentAssertions;

using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Terminology;

/// <summary>
/// The value object exists so no caller ever handles a raw language string —
/// which is also what lets it become a locale later without touching them.
/// </summary>
public sealed class LanguageCodeTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("EN", "en")]
    [InlineData("  fr  ", "fr")]
    [InlineData("De", "de")]
    public void ACodeIsTrimmedAndLowerCased(string input, string expected)
    {
        LanguageCode.Parse(input).Value.Should().Be(expected);
    }

    /// <summary>
    /// The point of normalising: "EN" and "en" are one language, so the
    /// one-name-per-language rule cannot be walked around by shouting.
    /// </summary>
    [Fact]
    public void CaseDoesNotMakeASecondLanguage()
    {
        LanguageCode.Parse("EN").Should().Be(LanguageCode.Parse("en"));

        LanguageCode.Parse("en").GetHashCode()
            .Should().Be(LanguageCode.Parse("EN").GetHashCode());
    }

    [Fact]
    public void DifferentLanguagesAreNotEqual()
    {
        LanguageCode.Parse("en").Should().NotBe(LanguageCode.Parse("fr"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AMissingCodeIsRefusedAsMissing(string? input)
    {
        var parse = () => LanguageCode.Parse(input);

        parse.Should().Throw<DomainException>()
            .WithMessage(LanguageCodeErrors.Required);
    }

    [Theory]
    [InlineData("e")]
    [InlineData("eng")]
    [InlineData("e1")]
    [InlineData("en-CA")]
    public void AnythingThatIsNotTwoLettersIsRefusedAsMalformed(string input)
    {
        var parse = () => LanguageCode.Parse(input);

        parse.Should().Throw<DomainException>()
            .WithMessage(LanguageCodeErrors.NotRecognised);
    }

    /// <summary>
    /// <c>en-CA</c> being refused today is the deliberate boundary, not an
    /// oversight: this models the minimum the domain currently reasons about.
    /// When regional variants earn a rule, the value object grows and no caller
    /// changes — that is the whole reason it exists.
    /// </summary>
    [Fact]
    public void TryParseReportsFailureRatherThanThrowing()
    {
        LanguageCode.TryParse("en-CA", out var locale).Should().BeFalse();
        locale.Should().BeNull();

        LanguageCode.TryParse("en", out var language).Should().BeTrue();
        language!.Value.Should().Be("en");
    }
}
