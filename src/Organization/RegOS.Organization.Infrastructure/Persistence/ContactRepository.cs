using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Persistence;

using ContactAggregate = RegOS.Organization.Domain.Aggregates.Contact.Contact;

namespace RegOS.Organization.Infrastructure.Persistence;

public sealed class ContactRepository : IContactRepository
{
    private readonly RegOSDbContext _dbContext;

    public ContactRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ContactAggregate contact,
        CancellationToken cancellationToken)
    {
        await _dbContext.Contacts.AddAsync(contact, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// The whole aggregate: a command that adds a role needs the existing ones
    /// loaded to enforce one-per-role.
    /// </summary>
    public async Task<ContactAggregate?> GetByIdAsync(
        ContactId id,
        CancellationToken cancellationToken)
        => await _dbContext.Contacts
            .Include(x => x.Roles)
            .Include(x => x.Emails)
            .Include(x => x.Phones)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        ContactAggregate contact,
        CancellationToken cancellationToken)
    {
        _dbContext.Contacts.Update(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
