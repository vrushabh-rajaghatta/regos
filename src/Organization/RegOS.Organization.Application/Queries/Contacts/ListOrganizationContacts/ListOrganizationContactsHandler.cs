using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Organization.Application.Queries.Contacts.ListOrganizationContacts;

public sealed class ListOrganizationContactsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListOrganizationContactsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Null when the organization does not exist, so the endpoint can 404
    /// rather than return an empty list for a company that was never there.
    /// </summary>
    public async Task<IReadOnlyList<ContactRow>?> HandleAsync(
        ListOrganizationContactsQuery query,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Organizations
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.OrganizationId, cancellationToken);

        if (!exists)
            return null;

        var contacts = await _dbContext.Contacts
            .AsNoTracking()
            .Where(x => x.OrganizationId == query.OrganizationId)
            .Include(x => x.Roles)
            .Include(x => x.Emails)
            .Include(x => x.Phones)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

        return await ContactProjection.ProjectAsync(
            _dbContext, contacts, cancellationToken);
    }
}
