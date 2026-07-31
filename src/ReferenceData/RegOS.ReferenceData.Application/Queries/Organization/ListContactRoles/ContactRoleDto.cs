namespace RegOS.ReferenceData.Application.Queries.Organization.ListContactRoles;

/// <param name="IsTenantOwn">
/// True for a role this tenant added, false for one the platform ships. The
/// distinction is worth showing: "Qualified Person" is defined by legislation
/// and "APAC Regulatory Lead" is one company's own word for a job.
/// </param>
public sealed record ContactRoleDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsTenantOwn);
