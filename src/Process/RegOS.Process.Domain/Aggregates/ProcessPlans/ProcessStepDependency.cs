namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

/// <summary>
/// One live step this step waits for.
/// </summary>
/// <remarks>
/// The plan-side counterpart of <c>ProcessStepPredecessor</c>, and a separate
/// type rather than a shared one: that points at a <c>ProcessStepDefinitionId</c>
/// and this at a <c>ProcessStepId</c>. Two different identity spaces, and a type
/// that blurred them would let a definition's graph be read as a plan's.
/// </remarks>
public sealed class ProcessStepDependency
{
    // EF materialisation.
    private ProcessStepDependency()
    {
    }

    internal ProcessStepDependency(ProcessStepId predecessorStepId)
    {
        PredecessorStepId = predecessorStepId;
    }

    public ProcessStepId PredecessorStepId { get; private set; } = default!;
}
