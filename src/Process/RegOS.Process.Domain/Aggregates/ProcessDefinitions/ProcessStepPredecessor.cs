namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

/// <summary>
/// One step this step waits for.
/// </summary>
/// <remarks>
/// <b>A type rather than a bare collection of ids</b>, because EF owns it as a
/// row in its own table and an owned entity needs somewhere to hang the
/// conversion. It carries nothing but the id it points at, and it is deliberately
/// not an entity: a predecessor edge has no identity of its own, and deleting
/// one leaves nothing behind to reference.
/// <para>
/// <b>The edge points backwards on purpose.</b> A step names what it waits for,
/// never what waits for it — so adding a step that depends on three existing ones
/// touches only the new step, and the successors a plan board draws are the
/// reverse of these, derived on read.
/// </para>
/// </remarks>
public sealed class ProcessStepPredecessor
{
    // EF materialisation.
    private ProcessStepPredecessor()
    {
    }

    internal ProcessStepPredecessor(ProcessStepDefinitionId predecessorStepId)
    {
        PredecessorStepId = predecessorStepId;
    }

    public ProcessStepDefinitionId PredecessorStepId { get; private set; } = default!;
}
