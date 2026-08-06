namespace RegOS.Process.Application.Queries.GetProcessDefinition;

/// <summary>
/// One version in the playbook's history.
/// </summary>
/// <param name="EffectiveTo">
/// <b>Derived, never stored</b> — the day before the next version took effect.
/// A second stored copy of that date is a thing that can drift from the version
/// that owns it, which is the defect <c>RegulatoryTemplateVersion.EffectiveTo</c>
/// has been carrying unset since it was written.
/// </param>
public sealed record ProcessDefinitionVersionSummary(
    Guid Id,
    int VersionNumber,
    string Status,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    DateTime? PublishedOnUtc,
    int StepCount);
