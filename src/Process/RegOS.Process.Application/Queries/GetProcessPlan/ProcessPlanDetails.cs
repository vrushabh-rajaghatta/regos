namespace RegOS.Process.Application.Queries.GetProcessPlan;

/// <param name="AnchorDate">
/// Half the answer to <em>"why is this milestone on this date?"</em>. The pinned
/// version is the other half.
/// </param>
/// <param name="DefinitionVersionIsSuperseded">
/// <b>Derived on read, stored nowhere</b> (ADR-065 D6). A newer version of the
/// playbook exists; this plan is unaffected and stays on the version it was
/// scheduled from. Disclosure, not a prompt to migrate.
/// </param>
public sealed record ProcessPlanDetails(
    Guid Id,
    string Name,
    string Status,
    Guid ProcessObjectiveId,
    string ObjectiveName,
    Guid ProcessDefinitionVersionId,
    string DefinitionName,
    int DefinitionVersionNumber,
    bool DefinitionVersionIsSuperseded,
    DateOnly AnchorDate,
    DateOnly OpenedOn,
    DateOnly? PlannedStartOn,
    DateOnly? PlannedEndOn,
    IReadOnlyList<PlannedStepDetails> Steps);
