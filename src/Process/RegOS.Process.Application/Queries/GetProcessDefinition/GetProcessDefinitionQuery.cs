namespace RegOS.Process.Application.Queries.GetProcessDefinition;

/// <summary>
/// One playbook, its versions, and the steps of the version asked for.
/// </summary>
/// <param name="VersionNumber">
/// Which version's steps to return. Null asks for the one a new plan would be
/// instantiated from — and, when nothing is published yet, the open draft, so a
/// playbook being written is still readable.
/// </param>
public sealed record GetProcessDefinitionQuery(Guid Id, int? VersionNumber = null);
