using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Organization.Application.Queries.Contacts.GetContact;

public sealed class GetContactHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetContactHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Null when the contact does not exist, or is invisible to this tenant —
    /// the fail-closed filter makes those the same answer (ADR-031).
    /// </summary>
    public async Task<ContactRow?> HandleAsync(
        GetContactQuery query,
        CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .AsNoTracking()
            .Include(x => x.Roles)
            .Include(x => x.Emails)
            .Include(x => x.Phones)
            .FirstOrDefaultAsync(x => x.Id == query.ContactId, cancellationToken);

        if (contact is null)
            return null;

        var rows = await ContactProjection.ProjectAsync(
            _dbContext, [contact], cancellationToken);

        return rows[0];
    }
}
