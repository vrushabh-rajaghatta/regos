using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// The commercial lifecycle of a market presence — deliberately less
/// constrained than a registration's, because commercial reality is.
/// </summary>
public sealed class MarketStatusTests
{
    private static readonly DateOnly Start = new(2019, 6, 1);

    private static MedicinalProduct New(DateOnly? statusDate = null)
        => MedicinalProduct.Create(
            TenantId.New(),
            GlobalProductId.New(),
            new CountryId(Guid.NewGuid()),
            statusDate ?? Start);

    // --- Creation ------------------------------------------------------------

    /// <summary>
    /// The first entry is the status it starts in, not a separate "created"
    /// event — the same shape Registration uses, so one history reads as one
    /// chronological sequence in one vocabulary.
    /// </summary>
    [Fact]
    public void AMarketBeginsPlannedAndSaysSoInItsHistory()
    {
        var market = New();

        market.CurrentMarketStatus.Should().Be(MarketStatus.Planned);

        var entry = market.MarketStatusHistory.Should().ContainSingle().Subject;

        entry.Status.Should().Be(MarketStatus.Planned);
        entry.OccurredOn.Should().Be(Start);
        entry.RecordedOnUtc.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// The provenance guarantee, one tier down from Registration's: a portfolio
    /// carried over from a legacy system says when things happened, not merely
    /// when they were typed in.
    /// </summary>
    [Fact]
    public void HistoryDistinguishesWhenItHappenedFromWhenRegOSLearned()
    {
        var market = New(new DateOnly(2015, 3, 2));
        market.ChangeMarketStatus(MarketStatus.Launched, new(2016, 9, 30));

        var launch = market.MarketStatusHistory
            .Single(entry => entry.Status == MarketStatus.Launched);

        launch.OccurredOn.Should().Be(new DateOnly(2016, 9, 30));
        launch.RecordedOnUtc.Date.Should().Be(DateTime.UtcNow.Date);
    }

    // --- The one incoherent transition ---------------------------------------

    /// <summary>
    /// <b>Why the initial state is Planned rather than NotLaunched.</b> "We
    /// intend to be here" cannot become true again once we are here, so the
    /// rule is enforced rather than explained in prose — where "not launched"
    /// would read as a reversible observation and need a warning instead.
    /// </summary>
    [Fact]
    public void AMarketAlreadyEnteredCannotBePlannedAgain()
    {
        var market = New();
        market.ChangeMarketStatus(MarketStatus.Launched, new(2020, 1, 1));

        var replan = () => market.ChangeMarketStatus(
            MarketStatus.Planned, new(2021, 1, 1));

        replan.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductErrors.MarketCannotBePlannedAgain);
    }

    // --- What is deliberately permitted --------------------------------------

    /// <summary>
    /// The whole reason there is no transition table. A regulator cannot
    /// approve twice; a company absolutely can launch twice, and forbidding it
    /// would encode one commercial history as universal law.
    /// </summary>
    [Fact]
    public void AMarketMayBeLaunchedLostAndLaunchedAgain()
    {
        var market = New();

        market.ChangeMarketStatus(MarketStatus.Launched, new(2020, 1, 1));
        market.ChangeMarketStatus(
            MarketStatus.TemporarilyUnavailable, new(2022, 4, 1),
            "API supply interruption.");
        market.ChangeMarketStatus(MarketStatus.Launched, new(2022, 11, 1));
        market.ChangeMarketStatus(MarketStatus.Discontinued, new(2025, 2, 1));

        market.CurrentMarketStatus.Should().Be(MarketStatus.Discontinued);

        market.MarketStatusHistory
            .OrderBy(entry => entry.OccurredOn)
            .Select(entry => entry.Status)
            .Should().Equal(
                MarketStatus.Planned,
                MarketStatus.Launched,
                MarketStatus.TemporarilyUnavailable,
                MarketStatus.Launched,
                MarketStatus.Discontinued);
    }

    /// <summary>
    /// Discontinued is not terminal, unlike a refused registration. A product
    /// genuinely can return to a market years later.
    /// </summary>
    [Fact]
    public void ADiscontinuedMarketMayBeRelaunched()
    {
        var market = New();
        market.ChangeMarketStatus(MarketStatus.Launched, new(2020, 1, 1));
        market.ChangeMarketStatus(MarketStatus.Discontinued, new(2021, 1, 1));

        var relaunch = () => market.ChangeMarketStatus(
            MarketStatus.Launched, new(2026, 1, 1));

        relaunch.Should().NotThrow();
    }

    // --- What coherence still forbids ----------------------------------------

    [Fact]
    public void AStatusCannotBeReEnteredFromItself()
    {
        var market = New();
        market.ChangeMarketStatus(MarketStatus.Launched, new(2020, 1, 1));

        var again = () => market.ChangeMarketStatus(
            MarketStatus.Launched, new(2021, 1, 1));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(
                MedicinalProductErrors.AlreadyInMarketStatus(
                    MarketStatus.Launched));
    }

    [Fact]
    public void BusinessTimeOnlyEverMovesForward()
    {
        var market = New(new DateOnly(2020, 1, 1));
        market.ChangeMarketStatus(MarketStatus.Launched, new(2021, 6, 1));

        var backdated = () => market.ChangeMarketStatus(
            MarketStatus.Discontinued, new(2020, 6, 1));

        backdated.Should().Throw<DomainException>()
            .WithMessage(MedicinalProductErrors.OccurredOnBeforePreviousEntry);
    }

    /// <summary>
    /// Two entries may share a date — a migration routinely produces that.
    /// </summary>
    [Fact]
    public void TwoEntriesMayShareABusinessDate()
    {
        var market = New(new DateOnly(2020, 1, 1));

        var sameDay = () => market.ChangeMarketStatus(
            MarketStatus.Launched, new(2020, 1, 1));

        sameDay.Should().NotThrow();
    }

    [Fact]
    public void ADateIsRequired()
    {
        var market = New();

        var undated = () => market.ChangeMarketStatus(
            MarketStatus.Launched, default);

        undated.Should().Throw<DomainException>()
            .WithMessage(MedicinalProductErrors.OccurredOnRequired);
    }

    [Fact]
    public void AnUnrecognisedStatusIsRefused()
    {
        var market = New();

        var nonsense = () => market.ChangeMarketStatus(
            (MarketStatus)99, new(2020, 1, 1));

        nonsense.Should().Throw<DomainException>()
            .WithMessage(MedicinalProductErrors.MarketStatusNotRecognised);
    }

    // --- The separation that must never blur ---------------------------------

    /// <summary>
    /// Operability and commercial state are different questions with different
    /// answers. A discontinued product's record is still perfectly in use.
    /// </summary>
    [Fact]
    public void CommercialStateDoesNotTouchWhetherTheRecordIsInUse()
    {
        var market = New();

        market.ChangeMarketStatus(MarketStatus.Discontinued, new(2025, 1, 1));

        market.Status.Should().Be(MedicinalProductStatus.Active);
        market.StatusDate.Should().Be(Start);
    }
}
