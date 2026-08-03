using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

using ContactAggregate = RegOS.Organization.Domain.Aggregates.Contact.Contact;

namespace RegOS.Organization.Application.Queries.Contacts;

/// <summary>
/// Turns contact aggregates into the rows every contact query returns.
/// </summary>
/// <remarks>
/// Shared by all three contact queries because they differ only in <em>which</em>
/// contacts they select, never in how a contact reads. Names are resolved once
/// for the whole page rather than per row.
/// </remarks>
public static class ContactProjection
{
    /// <summary>
    /// Names come from the referenced records, resolved once for the whole page
    /// rather than per row.
    /// </summary>
    public static async Task<IReadOnlyList<ContactRow>> ProjectAsync(
        RegOSDbContext dbContext,
        IReadOnlyList<ContactAggregate> contacts,
        CancellationToken cancellationToken)
    {
        if (contacts.Count == 0)
            return [];

        var organizationIds = contacts.Select(x => x.OrganizationId).Distinct().ToList();

        var organizations = await dbContext.Organizations
            .AsNoTracking()
            .Where(x => organizationIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.LegalName, cancellationToken);

        var siteIds = contacts
            .Where(x => x.OrganizationSiteId is not null)
            .Select(x => x.OrganizationSiteId!)
            .Distinct()
            .ToList();

        var sites = siteIds.Count == 0
            ? []
            : await dbContext.OrganizationSites
                .AsNoTracking()
                .Where(x => siteIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var roles = await dbContext.ContactRoles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => new { x.Code, x.Name }, cancellationToken);

        return contacts
            .Select(contact => new ContactRow(
                contact.Id.Value,
                contact.FirstName,
                contact.LastName,
                contact.Title,
                contact.Department,
                contact.OrganizationId.Value,
                organizations.GetValueOrDefault(
                    contact.OrganizationId, string.Empty),
                contact.OrganizationSiteId?.Value,
                contact.OrganizationSiteId is null
                    ? null
                    : sites.GetValueOrDefault(
                        contact.OrganizationSiteId, string.Empty),
                contact.Status.ToString(),
                contact.StatusDate,
                [.. contact.Roles
                    .Select(assignment => new ContactRoleDto(
                        assignment.RoleId.Value,
                        roles.GetValueOrDefault(assignment.RoleId)?.Code
                            ?? string.Empty,
                        roles.GetValueOrDefault(assignment.RoleId)?.Name
                            ?? string.Empty))
                    .OrderBy(x => x.Name)],
                [.. contact.Emails.Select(x => x.Address)],
                [.. contact.Phones.Select(x =>
                    new ContactPhoneDto(x.Number, x.Kind.ToString()))]))
            .ToList();
    }
}
