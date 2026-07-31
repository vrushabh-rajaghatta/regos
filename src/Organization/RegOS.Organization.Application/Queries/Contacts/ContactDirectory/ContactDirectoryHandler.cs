using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Organization.Application.Queries.Contacts.ContactDirectory;

/// <summary>
/// The query that makes <c>Contact</c> an aggregate root rather than a child of
/// Organization, so it ships in the same story the aggregate does.
/// </summary>
public sealed class ContactDirectoryHandler
{
    private readonly RegOSDbContext _dbContext;

    public ContactDirectoryHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ContactRow>> HandleAsync(
        ContactDirectoryQuery query,
        CancellationToken cancellationToken)
    {
        var contacts = _dbContext.Contacts.AsNoTracking();

        if (query.RoleId is { } role)
            contacts = contacts.Where(x => x.Roles.Any(r => r.RoleId == role));

        var rows = await contacts
            .Include(x => x.Roles)
            .Include(x => x.Emails)
            .Include(x => x.Phones)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

        return await ContactProjection.ProjectAsync(
            _dbContext, rows, cancellationToken);
    }
}
