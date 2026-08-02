namespace RegOS.Api.Endpoints.Submissions;

/// <param name="RoleId">
/// A <c>ContactRole</c> — the same vocabulary a contact's own roles draw on.
/// Naming someone here does not require their profile to list the role
/// (ADR-048).
/// </param>
public sealed record AssignSubmissionRoleRequest(
    Guid ContactId,
    Guid RoleId);
