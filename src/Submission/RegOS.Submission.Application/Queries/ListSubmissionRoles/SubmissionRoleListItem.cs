namespace RegOS.Submission.Application.Queries.ListSubmissionRoles;

/// <summary>
/// One person named on a filing, with the names the screen needs so reading a
/// submission's people never costs a second call.
/// </summary>
public sealed record SubmissionRoleListItem(
    Guid Id,
    Guid ContactId,
    string ContactName,
    string? ContactTitle,
    string OrganizationName,
    Guid RoleId,
    string RoleName);
