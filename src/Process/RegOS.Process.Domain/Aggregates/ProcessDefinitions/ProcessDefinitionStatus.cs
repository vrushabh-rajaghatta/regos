namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

/// <summary>
/// Whether a playbook is still one a team should be starting plans from.
/// </summary>
/// <remarks>
/// Lifecycle over deletion (ES-018). A retired playbook keeps every version it
/// ever published, because plans are pinned to those versions and a regulated
/// record may not lose the thing it was scheduled from.
/// </remarks>
public enum ProcessDefinitionStatus
{
    Active = 1,
    Retired = 2
}
