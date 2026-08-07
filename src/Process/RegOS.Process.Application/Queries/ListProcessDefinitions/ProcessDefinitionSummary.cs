namespace RegOS.Process.Application.Queries.ListProcessDefinitions;

/// <summary>One row of the playbook index.</summary>
/// <param name="IsShared">
/// True when this is the platform's playbook rather than the tenant's own. The
/// screen shows it, because a steward may extend the platform's set and may not
/// edit it (ADR-065 decision 7 / EPIC-012).
/// </param>
/// <param name="CurrentVersionNumber">
/// The version a new plan would be instantiated from, or null when nothing has
/// been published yet. **A playbook with only a draft is legitimate** — it is
/// being written.
/// </param>
public sealed record ProcessDefinitionSummary(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsShared,
    string CountryCode,
    string CountryName,
    string AuthorityName,
    string ApplicationTypeName,
    string Status,
    int? CurrentVersionNumber,
    int VersionCount,
    bool HasOpenDraft);
