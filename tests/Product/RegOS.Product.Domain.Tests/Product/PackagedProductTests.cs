using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// What a market sells — and the rules that keep a pack from contradicting
/// itself.
/// </summary>
public sealed class PackagedProductTests
{
    private static readonly DateOnly Start = new(2026, 1, 15);

    private static readonly CodedConcept Tablet =
        PharmaceuticalVocabulary.UnitOfPresentationOf("TABLET")!;

    private static PackagedProduct New(
        string? packCode = "0123-4567-89",
        DateOnly? statusDate = null)
        => Sized(30, Tablet, packCode, statusDate);

    /// <remarks>
    /// Quantity and unit are passed through exactly as given — no defaulting,
    /// because the tests below exist to supply one without the other.
    /// </remarks>
    private static PackagedProduct Sized(
        decimal? quantity,
        CodedConcept? unit,
        string? packCode = "0123-4567-89",
        DateOnly? statusDate = null)
        => PackagedProduct.Create(
            TenantId.New(),
            MedicinalProductId.New(),
            "Carton of 3 blisters × 10 film-coated tablets",
            quantity,
            unit,
            packCode,
            statusDate ?? Start);

    // --- what a pack is ------------------------------------------------------

    /// <summary>
    /// The first entry is the status it starts in, not a separate "created"
    /// event — one chronological sequence in one vocabulary, the shape
    /// <c>MedicinalProduct</c> and <c>Registration</c> already use.
    /// </summary>
    [Fact]
    public void APackBeginsPlannedAndSaysSoInItsHistory()
    {
        var pack = New();

        pack.CurrentMarketingStatus.Should().Be(PackageMarketingStatus.Planned);

        var entry = pack.MarketingStatusHistory.Should().ContainSingle().Subject;

        entry.Status.Should().Be(PackageMarketingStatus.Planned);
        entry.OccurredOn.Should().Be(Start);
    }

    /// <summary>
    /// The unit comes from the list a presentation and a component already use,
    /// not a fourth copy of it (ADR-061).
    /// </summary>
    [Fact]
    public void APackSizeCountsTheSameUnitAPresentationDoes()
    {
        New().PackSizeUnit!.Code.Should().Be("TABLET");
    }

    // --- half a pack size is not a pack size ---------------------------------

    /// <summary>
    /// <b>The guard names the ambiguity, not the field.</b> Same shape as a
    /// population's age band: <em>30</em> alone could be tablets, millilitres or
    /// vials.
    /// </summary>
    [Fact]
    public void AQuantityWithNoUnitIsRefused()
    {
        var create = () => Sized(30, null);

        create.Should().Throw<DomainException>()
            .WithMessage(PackagedProductErrors.PackSizeUnitRequired);
    }

    [Fact]
    public void AUnitWithNoQuantityIsRefused()
    {
        var create = () => Sized(null, Tablet);

        create.Should().Throw<DomainException>()
            .WithMessage(PackagedProductErrors.PackSizeQuantityRequired);
    }

    /// <summary>
    /// Neither is not missing data — it is a pack whose size is not settled,
    /// which a pack in design genuinely is.
    /// </summary>
    [Fact]
    public void NeitherIsOrdinary()
    {
        var pack = Sized(null, null);

        pack.PackSizeQuantity.Should().BeNull();
        pack.PackSizeUnit.Should().BeNull();
    }

    [Fact]
    public void AZeroPackSizeIsRefused()
    {
        var create = () => Sized(0, Tablet);

        create.Should().Throw<DomainException>()
            .WithMessage(PackagedProductErrors.PackSizeMustBePositive);
    }

    // --- the pack code -------------------------------------------------------

    /// <summary>
    /// A market issues it and RegOS does not, so there is no format rule. Null
    /// until the market issues one.
    /// </summary>
    [Fact]
    public void APackCodeIsOptionalAndUnvalidated()
    {
        New(packCode: null).PackCode.Should().BeNull();
        New(packCode: "PZN 12345678").PackCode.Should().Be("PZN 12345678");
    }

    // --- restating -----------------------------------------------------------

    /// <summary>
    /// The three facts are settled together: a corrected size that left the
    /// description saying <em>"carton of 30"</em> would be a pack contradicting
    /// itself.
    /// </summary>
    [Fact]
    public void RestatingSettlesDescriptionSizeAndCodeTogether()
    {
        var pack = New();

        pack.Describe(
            "Carton of 10 blisters × 10 film-coated tablets", 100, Tablet, "0123-4567-90");

        pack.Description.Should()
            .Be("Carton of 10 blisters × 10 film-coated tablets");
        pack.PackSizeQuantity.Should().Be(100);
        pack.PackCode.Should().Be("0123-4567-90");
    }

    /// <summary>
    /// Restating what a pack <em>is</em> does not touch its commercial history —
    /// the two move on different clocks, which is why they have separate routes.
    /// </summary>
    [Fact]
    public void RestatingLeavesTheHistoryUntouched()
    {
        var pack = New();

        pack.ChangeMarketingStatus(
            PackageMarketingStatus.Marketed, new DateOnly(2026, 3, 1));

        pack.Describe("Carton of 30 tablets", 30, Tablet, null);

        pack.CurrentMarketingStatus.Should().Be(PackageMarketingStatus.Marketed);
        pack.MarketingStatusHistory.Should().HaveCount(2);
    }

    [Fact]
    public void APackMustBeDescribed()
    {
        var pack = New();

        var describe = () => pack.Describe("   ", 30, Tablet, null);

        describe.Should().Throw<DomainException>()
            .WithMessage(PackagedProductErrors.DescriptionRequired);
    }

    // --- the commercial history ----------------------------------------------

    [Fact]
    public void EveryChangeUpdatesCurrentAndAppendsExactlyOneEntry()
    {
        var pack = New();

        pack.ChangeMarketingStatus(
            PackageMarketingStatus.Marketed, new DateOnly(2026, 3, 1));

        pack.CurrentMarketingStatus.Should().Be(PackageMarketingStatus.Marketed);
        pack.MarketingStatusHistory.Should().HaveCount(2);
    }

    /// <summary>
    /// <b>No transition table.</b> A pack size withdrawn from sale and
    /// reintroduced years later is ordinary commerce, and forbidding it would
    /// encode one company's history as universal law.
    /// </summary>
    [Fact]
    public void APackMayBeDiscontinuedAndMarketedAgain()
    {
        var pack = New();

        pack.ChangeMarketingStatus(
            PackageMarketingStatus.Marketed, new DateOnly(2026, 3, 1));
        pack.ChangeMarketingStatus(
            PackageMarketingStatus.Discontinued, new DateOnly(2027, 6, 1));
        pack.ChangeMarketingStatus(
            PackageMarketingStatus.Marketed, new DateOnly(2029, 1, 1));

        pack.CurrentMarketingStatus.Should().Be(PackageMarketingStatus.Marketed);
        pack.MarketingStatusHistory.Should().HaveCount(4);
    }

    /// <summary>
    /// The one genuinely incoherent transition: a pack that reached the market
    /// cannot be intended again.
    /// </summary>
    [Fact]
    public void APackCannotBePlannedAgain()
    {
        var pack = New();

        var replan = () => pack.ChangeMarketingStatus(
            PackageMarketingStatus.Planned, new DateOnly(2026, 3, 1));

        replan.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PackagedProductErrors.PackCannotBePlannedAgain);
    }

    [Fact]
    public void AStatusCannotBeReEnteredFromItself()
    {
        var pack = New();

        pack.ChangeMarketingStatus(
            PackageMarketingStatus.Marketed, new DateOnly(2026, 3, 1));

        var again = () => pack.ChangeMarketingStatus(
            PackageMarketingStatus.Marketed, new DateOnly(2026, 4, 1));

        again.Should().Throw<BusinessRuleViolationException>();
    }

    /// <summary>
    /// Business time moves forward. Discovering an earlier event later is a
    /// correction, which is a separate concept this does not offer.
    /// </summary>
    [Fact]
    public void AStatusCannotTakeEffectBeforeTheOneItReplaces()
    {
        var pack = New(statusDate: new DateOnly(2026, 3, 1));

        var backdate = () => pack.ChangeMarketingStatus(
            PackageMarketingStatus.Marketed, new DateOnly(2026, 1, 1));

        backdate.Should().Throw<DomainException>()
            .WithMessage(PackagedProductErrors.OccurredOnBeforePreviousEntry);
    }

    /// <summary>
    /// Two entries may share a date — a migration routinely produces that.
    /// </summary>
    [Fact]
    public void TwoEntriesMayShareADate()
    {
        var pack = New();

        var sameDay = () => pack.ChangeMarketingStatus(
            PackageMarketingStatus.Marketed, Start);

        sameDay.Should().NotThrow();
    }

    /// <summary>
    /// The two dates answer different questions: a pack discontinued in 2024 and
    /// entered today says 2024, and still records that RegOS learned it today.
    /// </summary>
    [Fact]
    public void TheBusinessDateAndTheEntryDateAreBothKept()
    {
        var pack = New(statusDate: new DateOnly(2024, 2, 1));

        pack.ChangeMarketingStatus(
            PackageMarketingStatus.Discontinued, new DateOnly(2024, 9, 30));

        var entry = pack.MarketingStatusHistory
            .Single(x => x.Status == PackageMarketingStatus.Discontinued);

        entry.OccurredOn.Should().Be(new DateOnly(2024, 9, 30));
        entry.RecordedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }
}
