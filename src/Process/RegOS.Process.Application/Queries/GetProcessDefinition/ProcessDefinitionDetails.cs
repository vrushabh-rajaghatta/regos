namespace RegOS.Process.Application.Queries.GetProcessDefinition;

/// <summary>
/// A playbook as a reader needs it: what it is for, every version it has been
/// through, and the steps of the one they are looking at.
/// </summary>
/// <param name="SelectedVersionNumber">
/// Which version <see cref="Steps"/> belongs to. Null when the playbook has no
/// versions at all, which is a playbook somebody has created and not yet started
/// writing.
/// </param>
public sealed record ProcessDefinitionDetails(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsShared,
    string CountryCode,
    string CountryName,
    string AuthorityCode,
    string AuthorityName,
    string ApplicationTypeCode,
    string ApplicationTypeName,
    string Status,
    DateTime CreatedOnUtc,
    IReadOnlyList<ProcessDefinitionVersionSummary> Versions,
    int? SelectedVersionNumber,
    IReadOnlyList<ProcessStepDetails> Steps);
