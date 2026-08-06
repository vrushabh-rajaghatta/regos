namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

/// <summary>
/// The lifecycle of one version of a playbook.
/// </summary>
/// <remarks>
/// <b>Three states, and the middle one is a one-way door.</b> A draft is being
/// written and may change freely. Publishing freezes it forever
/// ([ADR-065](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// I4), because a plan may already be scheduled from it. Superseding says only
/// that nothing new should instantiate from it — every plan already pinned to it
/// keeps working, unchanged and unmigrated.
/// </remarks>
public enum ProcessDefinitionVersionStatus
{
    Draft = 1,

    /// <summary>Frozen. Instantiable. Never editable again.</summary>
    Published = 2,

    /// <summary>Was in force; still readable; no longer instantiated from.</summary>
    Superseded = 3
}
