using FluentAssertions;

using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Tests;

/// <summary>
/// An objective's own rules — a lifecycle of <em>intent</em>, not of execution.
/// </summary>
/// <remarks>
/// The four examples that settled
/// [ADR-065 decision 3](../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// are all stateable with no schedule under them: <em>FDA approval for Product
/// X</em>, <em>CE MDR transition</em>, <em>expand an indication</em>, <em>renew a
/// licence</em>. That is what makes this a separate aggregate from the plan.
/// </remarks>
public class ProcessObjectiveTests
{
    private static readonly DateOnly Stated = new(2026, 8, 6);

    /// <summary>
    /// <b>An objective is stated before it is taken up.</b> There is no parameter
    /// that could create one already active — deciding to pursue something is a
    /// second, dated event.
    /// </summary>
    [Fact]
    public void A_new_objective_is_proposed_and_carries_one_history_entry()
    {
        var objective = AnObjective();

        objective.CurrentStatus.Should().Be(ProcessObjectiveStatus.Proposed);
        objective.History.Should().ContainSingle();
        objective.StatedOn.Should().Be(Stated);
        objective.AchievedOn.Should().BeNull();
    }

    /// <summary>
    /// The whole of D8: an objective exists before the market-local record does.
    /// </summary>
    [Fact]
    public void An_objective_needs_no_market_record()
    {
        AnObjective().MedicinalProductId.Should().BeNull();
    }

    [Fact]
    public void An_objective_moves_from_proposed_to_active_to_achieved()
    {
        var objective = AnObjective();

        objective.ChangeStatus(
            ProcessObjectiveStatus.Active, Stated.AddDays(30), "Funded.");
        objective.ChangeStatus(
            ProcessObjectiveStatus.Achieved, Stated.AddDays(400));

        objective.CurrentStatus.Should().Be(ProcessObjectiveStatus.Achieved);
        objective.AchievedOn.Should().Be(Stated.AddDays(400));
        objective.History.Should().HaveCount(3);
    }

    [Fact]
    public void An_achieved_objective_is_closed()
    {
        var objective = AnObjective();
        objective.ChangeStatus(ProcessObjectiveStatus.Achieved, Stated);

        var reopen = () => objective.ChangeStatus(
            ProcessObjectiveStatus.Active, Stated.AddDays(1));

        reopen.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessObjectiveErrors.AlreadyClosed);
    }

    /// <summary>
    /// An abandoned objective is kept, never deleted (ES-018). What a company
    /// decided not to pursue is as much a part of its record as what it did.
    /// </summary>
    [Fact]
    public void An_abandoned_objective_keeps_its_history()
    {
        var objective = AnObjective();

        objective.ChangeStatus(ProcessObjectiveStatus.Active, Stated.AddDays(10));
        objective.ChangeStatus(
            ProcessObjectiveStatus.Abandoned,
            Stated.AddDays(90),
            "Portfolio review deprioritised Japan.");

        objective.CurrentStatus.Should().Be(ProcessObjectiveStatus.Abandoned);
        objective.History.Should().HaveCount(3);
        objective.History[^1].Note.Should().Contain("deprioritised");
    }

    [Fact]
    public void An_objective_cannot_go_back_to_proposed()
    {
        var objective = AnObjective();
        objective.ChangeStatus(ProcessObjectiveStatus.Active, Stated);

        var backwards = () => objective.ChangeStatus(
            ProcessObjectiveStatus.Proposed, Stated.AddDays(1));

        backwards.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessObjectiveErrors.CannotReturnToProposed);
    }

    [Fact]
    public void History_cannot_be_recorded_out_of_order()
    {
        var objective = AnObjective();

        var earlier = () => objective.ChangeStatus(
            ProcessObjectiveStatus.Active, Stated.AddDays(-1));

        earlier.Should().Throw<DomainException>()
            .WithMessage(ProcessObjectiveErrors.HistoryOutOfOrder);
    }

    /// <summary>
    /// <b>The aggregate accepts the link without checking it, deliberately.</b>
    /// D8's rule — that the record must be for the same product and country — is
    /// a business rule the domain owns and cannot enforce here, because checking
    /// it means loading another aggregate (ADR-016). The command handler does it,
    /// and <c>ConfirmObjectiveMarketRecordTests</c> is where it is proven.
    /// </summary>
    [Fact]
    public void The_market_record_link_is_set_and_cleared_without_a_check()
    {
        var objective = AnObjective();
        var market = MedicinalProductId.New();

        objective.ConfirmMarketRecord(market);
        objective.MedicinalProductId.Should().Be(market);

        objective.ConfirmMarketRecord(null);
        objective.MedicinalProductId.Should().BeNull();
    }

    [Fact]
    public void An_objective_is_about_a_product_and_a_market()
    {
        var noCountry = () => ProcessObjective.Create(
            new TenantId(Guid.NewGuid()),
            GlobalProductId.New(),
            default,
            "Approve in Japan",
            Stated);

        noCountry.Should().Throw<DomainException>()
            .WithMessage(ProcessObjectiveErrors.CountryRequired);
    }

    [Fact]
    public void An_objective_says_what_it_is_trying_to_achieve()
    {
        var unnamed = () => ProcessObjective.Create(
            new TenantId(Guid.NewGuid()),
            GlobalProductId.New(),
            new CountryId(Guid.NewGuid()),
            "   ",
            Stated);

        unnamed.Should().Throw<DomainException>()
            .WithMessage(ProcessObjectiveErrors.NameRequired);
    }

    private static ProcessObjective AnObjective()
        => ProcessObjective.Create(
            new TenantId(Guid.NewGuid()),
            GlobalProductId.New(),
            new CountryId(Guid.NewGuid()),
            "Obtain approval in Japan",
            Stated,
            "505(b)(1) route, pre-IND meeting first.");
}
