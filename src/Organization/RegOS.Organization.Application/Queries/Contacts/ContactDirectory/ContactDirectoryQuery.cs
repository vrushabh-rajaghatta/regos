using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Organization.Application.Queries.Contacts.ContactDirectory;

/// <summary>
/// "Who holds this role?" — across the tenant's whole registry.
/// </summary>
/// <param name="RoleId">
/// Optional, and there is no default: with no role the directory returns
/// everyone. Nothing is hidden either way — an inactive contact is returned and
/// marked, because the person named on a 2019 licence is still that person.
/// </param>
public sealed record ContactDirectoryQuery(ContactRoleId? RoleId = null);
