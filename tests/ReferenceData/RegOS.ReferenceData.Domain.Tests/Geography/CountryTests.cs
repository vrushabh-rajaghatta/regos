using FluentAssertions;

using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Geography;

/// <summary>
/// EPIC-022 S001 — the two ISO identity fields machine-readable output needs.
/// </summary>
/// <remarks>
/// <b>Two names and two codes, for two audiences.</b> A person picks
/// <em>"United Kingdom"</em> from a list; an xEVMPD message names
/// <em>GBR / "United Kingdom of Great Britain and Northern Ireland"</em>.
/// Neither pair is derivable from the other, which is the whole reason all four
/// are stored.
/// </remarks>
public sealed class CountryTests
{
    private static Country A(
        string code = "GB",
        string alpha3 = "GBR",
        string name = "United Kingdom",
        string isoName = "United Kingdom of Great Britain and Northern Ireland",
        IEnumerable<CodedConcept>? regions = null)
        => Country.Create(CountryId.New(), code, alpha3, name, isoName, regions);

    [Fact]
    public void ACountryCarriesBothCodesAndBothNames()
    {
        var country = A();

        country.Code.Should().Be("GB");
        country.IsoAlpha3Code.Should().Be("GBR");
        country.Name.Should().Be("United Kingdom");
        country.IsoName.Should()
            .Be("United Kingdom of Great Britain and Northern Ireland");
    }

    /// <summary>
    /// Both codes are upper-cased on the way in, so <c>gbr</c> and <c>GBR</c>
    /// cannot become two different countries.
    /// </summary>
    [Fact]
    public void CodesAreNormalisedUpward()
    {
        var country = A(code: " gb ", alpha3: " gbr ");

        country.Code.Should().Be("GB");
        country.IsoAlpha3Code.Should().Be("GBR");
    }

    /// <summary>
    /// <b>The mistake worth catching.</b> The two code columns are one keystroke
    /// apart, and an alpha-2 value in the alpha-3 column would be carried into
    /// every downstream message without anything noticing.
    /// </summary>
    [Theory]
    [InlineData("GB")]
    [InlineData("GBRA")]
    [InlineData("G1B")]
    [InlineData("G B")]
    public void AnAlphaThreeCodeThatIsNotThreeLettersIsRefused(string malformed)
    {
        var create = () => A(alpha3: malformed);

        create.Should().Throw<DomainException>()
            .WithMessage(CountryErrors.IsoAlpha3CodeMalformed);
    }

    [Fact]
    public void AnAlphaThreeCodeIsRequired()
    {
        var create = () => A(alpha3: "  ");

        create.Should().Throw<DomainException>()
            .WithMessage(CountryErrors.IsoAlpha3CodeRequired);
    }

    [Fact]
    public void AnIsoNameIsRequired()
    {
        var create = () => A(isoName: "  ");

        create.Should().Throw<DomainException>()
            .WithMessage(CountryErrors.IsoNameRequired);
    }

    /// <summary>
    /// The two names are allowed to be identical — most countries are — and
    /// allowed to differ. Neither case is special.
    /// </summary>
    [Fact]
    public void TheCommonNameAndTheIsoNameMayAgreeOrDiffer()
    {
        A(code: "FR", alpha3: "FRA", name: "France", isoName: "France")
            .IsoName.Should().Be("France");

        A().IsoName.Should().NotBe(A().Name);
    }

    /// <summary>
    /// <b>Flat master data, and the collections EPIC-022 adds do not change
    /// that</b> (ADR-043 §2). No lifecycle, no behaviour beyond
    /// <c>Create</c> — so the identity stays a record struct until something
    /// gives a country a lifecycle.
    /// </summary>
    [Fact]
    public void ACountryHasNoLifecycleToChange()
    {
        // Property accessors excluded: it is behaviour this is looking for,
        // and a getter is not behaviour.
        typeof(Country).GetMethods()
            .Where(x => x.IsPublic
                && !x.IsStatic
                && !x.IsSpecialName
                && x.DeclaringType == typeof(Country))
            .Should().BeEmpty();
    }
    // --- the groupings a country belongs to ----------------------------------

    private static CodedConcept Region(string code)
        => GeographyVocabulary.RegionOf(code)!;

    /// <summary>
    /// <b>They overlap, and that is why this is a collection.</b> Germany is EU
    /// and ICH and PIC/S — which the single nullable `RegionCode` this replaces
    /// could not have said even if anything had ever written to it.
    /// </summary>
    [Fact]
    public void ACountryMayBelongToSeveralGroupingsAtOnce()
    {
        var germany = A(code: "DE", alpha3: "DEU", name: "Germany", isoName: "Germany",
            regions: [Region("EU"), Region("ICH"), Region("PIC_S")]);

        germany.Regions.Should().HaveCount(3);
    }

    /// <summary>
    /// <b>Empty is a recorded answer, not an unfilled field.</b> India belongs
    /// to none of the five: CDSCO is an ICH <em>observer</em> rather than a
    /// member, and India is not a PIC/S participant.
    /// </summary>
    [Fact]
    public void ACountryMayBelongToNoGroupingAtAll()
    {
        A(code: "IN", alpha3: "IND", name: "India", isoName: "India", regions: [])
            .Regions.Should().BeEmpty();
    }

    [Fact]
    public void TheSameGroupingTwiceIsRefused()
    {
        var create = () => A(regions: [Region("ICH"), Region("ICH")]);

        create.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(CountryErrors.RegionAlreadyStated);
    }

    /// <summary>
    /// <b>Membership is a dated fact dressed as geography.</b> The United
    /// Kingdom was EU and is not, which is why nothing here derives a grouping
    /// from a country's location.
    /// </summary>
    [Fact]
    public void MembershipIsRecordedRatherThanDerivedFromGeography()
    {
        var uk = A(regions: [Region("ICH"), Region("PIC_S")]);

        uk.Regions.Select(x => x.Code).Should().NotContain("EU");
    }

}
