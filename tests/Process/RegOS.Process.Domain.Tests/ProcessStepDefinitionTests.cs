using FluentAssertions;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Domain.Tests;

/// <summary>
/// The step graph, and the guarantee publication makes about it.
/// </summary>
/// <remarks>
/// <b>Publication certifies that the definition is a valid DAG suitable for plan
/// instantiation</b> — a stronger statement than <em>"cycles are not allowed"</em>,
/// and the reason S003 never has to validate a graph it instantiates from
/// (ADR-065 decision 4).
/// </remarks>
public class ProcessStepDefinitionTests
{
    [Fact]
    public void A_step_code_is_unique_within_its_version()
    {
        var definition = Drafted();
        definition.AddStep("SUBMIT", "Transmit the IND");

        var duplicate = () => definition.AddStep("submit", "Something else");

        duplicate.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.DuplicateStepCode);
    }

    /// <summary>
    /// The same code in a <em>different</em> version is the normal case — a
    /// version is a restatement of the same process.
    /// </summary>
    [Fact]
    public void The_same_code_may_appear_in_two_versions()
    {
        var definition = Drafted();
        definition.AddStep("SUBMIT", "Transmit the IND");
        definition.PublishVersion(
            definition.Draft!.Id, null, DateTime.UtcNow);

        definition.StartDraftVersion();

        var again = () => definition.AddStep("SUBMIT", "Transmit the IND");

        again.Should().NotThrow();
    }

    [Fact]
    public void A_predecessor_must_belong_to_the_same_version()
    {
        var definition = Drafted();
        var step = definition.AddStep("SUBMIT", "Transmit the IND");

        var stray = () => definition.AddStepPredecessor(
            step.Id, ProcessStepDefinitionId.New());

        stray.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.PredecessorNotFound);
    }

    [Fact]
    public void A_step_cannot_wait_for_itself()
    {
        var definition = Drafted();
        var step = definition.AddStep("SUBMIT", "Transmit the IND");

        var itself = () => definition.AddStepPredecessor(step.Id, step.Id);

        itself.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.StepCannotPrecedeItself);
    }

    [Fact]
    public void A_parent_step_must_belong_to_the_same_version()
    {
        var definition = Drafted();

        var orphan = () => definition.AddStep(
            "SUBMIT", "Transmit the IND",
            parentStepId: ProcessStepDefinitionId.New());

        orphan.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.ParentStepNotFound);
    }

    [Fact]
    public void Steps_group_into_phases_through_a_parent()
    {
        var definition = Drafted();
        var phase = definition.AddStep("PRE-IND", "Pre-IND phase");

        var child = definition.AddStep(
            "PRE-IND-REQ", "Submit the meeting request", parentStepId: phase.Id);

        child.ParentStepId.Should().Be(phase.Id);
    }

    [Fact]
    public void A_step_cannot_start_before_what_it_waits_for()
    {
        var definition = Drafted();

        var backwards = () => definition.AddStep(
            "SUBMIT", "Transmit the IND", offsetDays: -1);

        backwards.Should().Throw<DomainException>()
            .WithMessage(ProcessDefinitionErrors.OffsetDaysNegative);
    }

    [Fact]
    public void A_step_takes_at_least_a_day()
    {
        var definition = Drafted();

        var instant = () => definition.AddStep(
            "SUBMIT", "Transmit the IND", durationDays: 0);

        instant.Should().Throw<DomainException>()
            .WithMessage(ProcessDefinitionErrors.DurationDaysNotPositive);
    }

    /// <summary>
    /// <b>The whole reason the check lives at publish.</b> A circle makes every
    /// start date underivable, and the playbook — not the plan somebody tried to
    /// make from it — is the thing that is wrong.
    /// </summary>
    [Fact]
    public void A_circle_of_predecessors_cannot_be_published()
    {
        var definition = Drafted();
        var first = definition.AddStep("A", "First");
        var second = definition.AddStep("B", "Second");
        var third = definition.AddStep("C", "Third");

        definition.AddStepPredecessor(second.Id, first.Id);
        definition.AddStepPredecessor(third.Id, second.Id);
        definition.AddStepPredecessor(first.Id, third.Id);

        var publish = () => definition.PublishVersion(
            definition.Draft!.Id, null, DateTime.UtcNow);

        publish.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.PredecessorCycle);
    }

    /// <summary>
    /// The converging shape the FDA IND playbook actually has: three independent
    /// strands meeting at compilation. A diamond is not a cycle, and a guard that
    /// confused the two would refuse every real playbook.
    /// </summary>
    [Fact]
    public void A_converging_graph_publishes()
    {
        var definition = Drafted();
        var start = definition.AddStep("CMC", "Complete the CMC package");
        var left = definition.AddStep("IB", "Investigator's Brochure");
        var right = definition.AddStep("FORMS", "FDA forms");
        var join = definition.AddStep("COMPILE", "Compile the sequence");

        definition.AddStepPredecessor(left.Id, start.Id);
        definition.AddStepPredecessor(right.Id, start.Id);
        definition.AddStepPredecessor(join.Id, left.Id);
        definition.AddStepPredecessor(join.Id, right.Id);

        var publish = () => definition.PublishVersion(
            definition.Draft!.Id, null, DateTime.UtcNow);

        publish.Should().NotThrow();
    }

    [Fact]
    public void A_step_waits_for_another_at_most_once()
    {
        var definition = Drafted();
        var first = definition.AddStep("A", "First");
        var second = definition.AddStep("B", "Second");

        definition.AddStepPredecessor(second.Id, first.Id);
        var again = () => definition.AddStepPredecessor(second.Id, first.Id);

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.DuplicatePredecessor);
    }

    private static ProcessDefinition Drafted()
    {
        var definition = ProcessDefinitionTests.ADefinition();
        definition.StartDraftVersion();

        return definition;
    }
}
