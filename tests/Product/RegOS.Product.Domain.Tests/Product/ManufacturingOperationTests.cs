using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// EPIC-010c S001 — a site performing one operation for one market, over a
/// period.
/// </summary>
/// <remarks>
/// <b>The single place that says where work happens</b>
/// (<see href="../../../../docs/adr/ADR-063-where-a-product-is-made-is-a-product-fact.md">ADR-063</see>
/// §3). RIM puts a <c>Manufacturer</c> on <c>Packaging</c> and another on
/// <c>Packaged Product</c>; RegOS keeps neither, because the distinction those
/// columns drew — who packs it, who tests it, who releases it — is carried by
/// the operation's own type.
/// </remarks>
public sealed class ManufacturingOperationTests
{
    private static readonly TenantId Tenant = new(Guid.NewGuid());
    private static readonly MedicinalProductId Market = MedicinalProductId.New();
    private static readonly OrganizationSiteId Gamma = OrganizationSiteId.New();

    private static CodedConcept Operation(string code)
        => ManufacturingVocabulary.OperationOf(code)!;

    private static ManufacturingOperation A(
        OrganizationSiteId? site = null,
        string operation = "FINISHED_PRODUCT",
        DateOnly? from = null)
        => ManufacturingOperation.Record(
            Tenant,
            Market,
            site ?? Gamma,
            Operation(operation),
            from ?? new DateOnly(2024, 3, 1));

    // --- the record ----------------------------------------------------------

    [Fact]
    public void AnOperationIsASiteAnActAndAPeriod()
    {
        var operation = A();

        operation.OrganizationSiteId.Should().Be(Gamma);
        operation.Operation.Code.Should().Be("FINISHED_PRODUCT");
        operation.EffectiveFrom.Should().Be(new DateOnly(2024, 3, 1));
        operation.CeasedOn.Should().BeNull();
        operation.IsCurrent.Should().BeTrue();
    }

    /// <summary>
    /// <b>Supplied, not read from the clock.</b> An operation recorded today
    /// can say it has run since 2019 — the same call <c>OrganizationSite</c>
    /// makes about its status date.
    /// </summary>
    [Fact]
    public void AnOperationMayHaveStartedLongBeforeItWasRecorded()
    {
        A(from: new DateOnly(2019, 1, 1))
            .EffectiveFrom.Should().Be(new DateOnly(2019, 1, 1));
    }

    [Fact]
    public void AnOperationNeedsAStartDate()
    {
        var record = () => ManufacturingOperation.Record(
            Tenant, Market, Gamma, Operation("QC_TESTING"), default);

        record.Should().Throw<DomainException>()
            .WithMessage(ManufacturingOperationErrors.EffectiveFromRequired);
    }

    /// <summary>
    /// <b>An operation with no type says a site is involved without saying
    /// how</b>, which is not a record anybody can act on.
    /// </summary>
    [Fact]
    public void AnOperationNeedsAnAct()
    {
        var record = () => ManufacturingOperation.Record(
            Tenant, Market, Gamma, null!, new DateOnly(2024, 3, 1));

        record.Should().Throw<DomainException>()
            .WithMessage(ManufacturingOperationErrors.OperationRequired);
    }

    // --- the period ----------------------------------------------------------

    /// <summary>
    /// <b>Closed, never deleted</b> (ES-018). A site that made this product for
    /// four years made it, and removing the row would make a 2023 filing
    /// unexplainable.
    /// </summary>
    [Fact]
    public void CeasingClosesThePeriodAndKeepsTheRow()
    {
        var operation = A(from: new DateOnly(2019, 1, 1));

        operation.Cease(new DateOnly(2023, 6, 30));

        operation.CeasedOn.Should().Be(new DateOnly(2023, 6, 30));
        operation.IsCurrent.Should().BeFalse();
        operation.EffectiveFrom.Should().Be(new DateOnly(2019, 1, 1));
        operation.OrganizationSiteId.Should().Be(Gamma);
    }

    [Fact]
    public void AnOperationCannotStopBeforeItStarts()
    {
        var operation = A(from: new DateOnly(2024, 3, 1));

        var cease = () => operation.Cease(new DateOnly(2024, 2, 29));

        cease.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ManufacturingOperationErrors.CeasedBeforeItStarted);
    }

    [Fact]
    public void ClosingAClosedPeriodIsRefused()
    {
        var operation = A();
        operation.Cease(new DateOnly(2025, 1, 1));

        var again = () => operation.Cease(new DateOnly(2025, 6, 1));

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ManufacturingOperationErrors.AlreadyCeased);
    }

    /// <summary>
    /// <b>A transfer is two rows, not an edited one.</b> The site and the act
    /// are immutable for the same reason <c>PackAuthorisation</c>'s pair is:
    /// editing one into another leaves no way to tell a correction from a
    /// transfer, and *"who released our batches in 2023?"* stops being
    /// answerable.
    /// </summary>
    [Fact]
    public void ATransferClosesOnePeriodAndOpensAnother()
    {
        var delta = OrganizationSiteId.New();

        var was = A(from: new DateOnly(2019, 1, 1));
        was.Cease(new DateOnly(2024, 2, 29));

        var now = A(site: delta, from: new DateOnly(2024, 3, 1));

        was.OrganizationSiteId.Should().Be(Gamma);
        was.IsCurrent.Should().BeFalse();
        now.OrganizationSiteId.Should().Be(delta);
        now.IsCurrent.Should().BeTrue();
    }

    [Fact]
    public void CorrectingTheDatesLeavesTheSiteAndTheActAlone()
    {
        var operation = A();

        operation.Correct(new DateOnly(2024, 1, 1), new DateOnly(2025, 1, 1));

        operation.EffectiveFrom.Should().Be(new DateOnly(2024, 1, 1));
        operation.CeasedOn.Should().Be(new DateOnly(2025, 1, 1));
        operation.OrganizationSiteId.Should().Be(Gamma);
        operation.Operation.Code.Should().Be("FINISHED_PRODUCT");
    }

    [Fact]
    public void ACorrectionCannotEndBeforeItBegins()
    {
        var operation = A();

        var correct = () =>
            operation.Correct(new DateOnly(2024, 3, 1), new DateOnly(2024, 1, 1));

        correct.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ManufacturingOperationErrors.CeasedBeforeItStarted);
    }

    // --- what it deliberately does not do ------------------------------------

    /// <summary>
    /// <b>D4's test, asserted rather than remembered.</b> Operation type is
    /// data, not an enum, and the moment a rule reads its code to decide
    /// something it has stopped being vocabulary — which is the test
    /// <c>OrganizationSiteType</c>'s docstring records for going the other way.
    /// <para>
    /// Checked structurally: every operation type is accepted by the same
    /// methods with the same outcome, so no branch can exist.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("API_MANUFACTURE")]
    [InlineData("FINISHED_PRODUCT")]
    [InlineData("PRIMARY_PACKAGING")]
    [InlineData("SECONDARY_PACKAGING")]
    [InlineData("QC_TESTING")]
    [InlineData("BATCH_RELEASE")]
    [InlineData("IMPORTATION")]
    public void NoRuleBranchesOnWhichOperationItIs(string code)
    {
        var operation = A(operation: code);

        operation.Operation.Code.Should().Be(code);
        operation.IsCurrent.Should().BeTrue();

        operation.Cease(new DateOnly(2025, 1, 1));

        operation.IsCurrent.Should().BeFalse();
    }

    /// <summary>
    /// <b>The vocabulary covers every operation separately authorised in the
    /// real world</b>, and the primary/secondary packaging split is not
    /// cosmetic: primary packaging touches the product, secondary is the carton
    /// and is frequently done locally per market.
    /// </summary>
    [Fact]
    public void ThePackagingSplitIsTwoTerms()
    {
        ManufacturingVocabulary.Operations.Select(x => x.Code).Should()
            .Contain(["PRIMARY_PACKAGING", "SECONDARY_PACKAGING"]);

        ManufacturingVocabulary.OperationOf("PACKAGING").Should().BeNull();
    }

    /// <summary>
    /// <b>Recording is not approving</b> (EPIC-010c D6). This type says the work
    /// happens; whether a licence permits it is a different aggregate's
    /// statement, and the gap between them is what S004 reports. Asserted
    /// structurally: there is nothing here that could refuse on that basis.
    /// </summary>
    [Fact]
    public void NothingHereKnowsWhetherALicenceApprovesIt()
    {
        typeof(ManufacturingOperation).GetProperties()
            .Select(x => x.Name)
            .Should().NotContain(name =>
                name.Contains("Registration", StringComparison.Ordinal)
                || name.Contains("Approv", StringComparison.Ordinal)
                || name.Contains("Licence", StringComparison.Ordinal));
    }
}
