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
        IEnumerable<CodedConcept>? regions = null,
        IEnumerable<LanguageCode>? languages = null,
        IEnumerable<CodedConcept>? stabilityConditions = null)
        => Country.Create(
            CountryId.New(), code, alpha3, name, isoName,
            regions, languages, stabilityConditions);

    /// <summary>
    /// Every public instance method that returns nothing — which is what a
    /// state transition looks like.
    /// </summary>
    /// <remarks>
    /// <b>Sharpened in S004, and deliberately not weakened.</b> This asked for
    /// <em>no public instance methods at all</em> until <c>Country</c> gained
    /// <c>AcceptsStabilityDataFrom</c> — a pure question that reads two
    /// collections and changes nothing. ADR-043 §2's test is
    /// <em>identity semantics</em>: children with identity and a lifecycle. A
    /// method that answers a question is neither, and a <c>void Deactivate()</c>
    /// would still be caught here.
    /// </remarks>
    private static IEnumerable<string> StateTransitionsOn<T>()
        => typeof(T).GetMethods()
            .Where(x => x.IsPublic
                && !x.IsStatic
                && !x.IsSpecialName
                && x.DeclaringType == typeof(T)
                && x.ReturnType == typeof(void))
            .Select(x => x.Name);

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
        StateTransitionsOn<Country>().Should().BeEmpty();
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
    // --- the languages its labelling is expected in --------------------------

    /// <summary>
    /// <b>The debt EPIC-018 could not close itself.</b> `LocalLabel.Language`
    /// existed and nothing could say which languages a market needed, so nobody
    /// could be told a Canadian label set was incomplete.
    /// </summary>
    [Fact]
    public void AMarketMayExpectItsLabellingInTwoLanguages()
    {
        var canada = A(code: "CA", alpha3: "CAN", name: "Canada", isoName: "Canada",
            languages: [LanguageCode.Parse("en"), LanguageCode.Parse("fr")]);

        canada.Languages.Select(x => x.Value).Should().BeEquivalentTo(["en", "fr"]);
    }

    [Fact]
    public void TheSameLanguageTwiceIsRefused()
    {
        var create = () => A(
            languages: [LanguageCode.Parse("en"), LanguageCode.Parse("EN")]);

        create.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(CountryErrors.LanguageAlreadyStated);
    }

    /// <summary>
    /// <b>Nothing here refuses anything about labelling</b> (EPIC-022 D4). The
    /// country states what its labelling is normally in; whether a given label
    /// must be in all of them depends on the product and the document, and a
    /// country knows neither. Asserted structurally: no behaviour to enforce
    /// with.
    /// </summary>
    [Fact]
    public void ACountryStatesItsLanguagesAndEnforcesNothing()
    {
        var canada = A(languages: [LanguageCode.Parse("en"), LanguageCode.Parse("fr")]);

        canada.Languages.Should().HaveCount(2);

        StateTransitionsOn<Country>().Should().BeEmpty();
    }

    // --- the stability conditions its market accepts -------------------------

    private static CodedConcept Condition(string code)
        => StabilityVocabulary.ConditionOf(code)!;

    private static Country Germany(params string[] conditions)
        => A(code: "DE", alpha3: "DEU", name: "Germany", isoName: "Germany",
            stabilityConditions: [.. conditions.Select(Condition)]);

    /// <summary>
    /// <b>Conditions, never a climatic zone</b> (EPIC-022 D6). WHO publishes
    /// the long-term testing condition each member state accepts and declines
    /// to publish a zone letter per country — and <b>India accepts 30 °C/70% RH,
    /// which is neither Zone IVA (30/65) nor Zone IVB (30/75)</b>. A zone field
    /// would hold RegOS's reading of WHO rather than WHO.
    /// </summary>
    [Fact]
    public void AMarketStatesTheConditionsItAcceptsAndNotAZone()
    {
        var india = A(code: "IN", alpha3: "IND", name: "India", isoName: "India",
            stabilityConditions: [Condition("30C_70RH")]);

        india.StabilityConditions.Select(x => x.Code).Should()
            .BeEquivalentTo(["30C_70RH"]);
    }

    /// <summary>
    /// <b>Several, because WHO's table says "or".</b> Seven of the eight seeded
    /// markets accept 25 °C/60% RH <em>or</em> 30 °C/65% RH, which is why the
    /// match below is an overlap rather than an equality.
    /// </summary>
    [Fact]
    public void AMarketMayAcceptEitherOfTwoConditions()
    {
        Germany("25C_60RH", "30C_65RH").StabilityConditions.Should().HaveCount(2);
    }

    [Fact]
    public void TheSameStabilityConditionTwiceIsRefused()
    {
        var create = () => Germany("25C_60RH", "25C_60RH");

        create.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(CountryErrors.StabilityConditionAlreadyStated);
    }

    /// <summary>
    /// <b>The rule, and the only place it lives:</b> a pack is suitable for a
    /// market if at least one of the conditions its shelf life was demonstrated
    /// at is accepted there.
    /// </summary>
    [Fact]
    public void OneAcceptedConditionIsEnough()
    {
        Germany("25C_60RH", "30C_65RH")
            .AcceptsStabilityDataFrom([Condition("30C_75RH"), Condition("25C_60RH")])
            .Should().BeTrue();
    }

    /// <summary>
    /// <b>The row the story turns on.</b> A pack whose data was generated at
    /// 25 °C/60% RH is supported in Germany and not in India — same pack, same
    /// number, different market — and India's condition belongs to no climatic
    /// zone anybody publishes.
    /// </summary>
    [Fact]
    public void GermanyAcceptsWhatIndiaDoesNot()
    {
        var temperate = new[] { Condition("25C_60RH") };

        Germany("25C_60RH", "30C_65RH")
            .AcceptsStabilityDataFrom(temperate).Should().BeTrue();

        A(code: "IN", alpha3: "IND", name: "India", isoName: "India",
                stabilityConditions: [Condition("30C_70RH")])
            .AcceptsStabilityDataFrom(temperate).Should().BeFalse();
    }

    /// <summary>
    /// <b>Silence is not a refusal, and that is why the answer is
    /// three-valued.</b> A pack whose stability data has not been recorded is
    /// not a pack whose data is rejected; a market RegOS holds no conditions
    /// for cannot reject anything. Either collapsed to <c>false</c> would put
    /// a warning on screen that means <em>"we do not know"</em>.
    /// </summary>
    [Fact]
    public void NothingStatedOnEitherSideIsNotARejection()
    {
        Germany("25C_60RH")
            .AcceptsStabilityDataFrom([]).Should().BeNull();

        Germany("25C_60RH")
            .AcceptsStabilityDataFrom(null).Should().BeNull();

        Germany()
            .AcceptsStabilityDataFrom([Condition("25C_60RH")]).Should().BeNull();
    }

    /// <summary>
    /// <b>Reported, never enforced</b> (EPIC-005's expiry precedent). Asking
    /// the question cannot change the country, and there is no method here that
    /// refuses anything — the verdict is a value a screen renders.
    /// </summary>
    [Fact]
    public void AskingTheQuestionChangesNothingAndRefusesNothing()
    {
        var germany = Germany("25C_60RH", "30C_65RH");

        germany.AcceptsStabilityDataFrom([Condition("30C_70RH")])
            .Should().BeFalse();

        germany.StabilityConditions.Should().HaveCount(2);
        StateTransitionsOn<Country>().Should().BeEmpty();
    }
}
