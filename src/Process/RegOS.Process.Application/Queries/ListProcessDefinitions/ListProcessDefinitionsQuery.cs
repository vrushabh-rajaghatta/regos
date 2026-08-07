namespace RegOS.Process.Application.Queries.ListProcessDefinitions;

/// <summary>
/// Every playbook this tenant can see — the platform's, plus its own.
/// </summary>
/// <param name="IncludeRetired">
/// Retired playbooks are hidden by default. They are never deleted (ES-018),
/// because plans stay pinned to versions they published.
/// </param>
public sealed record ListProcessDefinitionsQuery(bool IncludeRetired = false);
