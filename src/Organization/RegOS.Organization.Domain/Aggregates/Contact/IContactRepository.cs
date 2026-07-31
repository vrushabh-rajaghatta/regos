namespace RegOS.Organization.Domain.Aggregates.Contact;

using ContactAggregate = RegOS.Organization.Domain.Aggregates.Contact.Contact;

/// <summary>
/// Aggregates only. Reads for screens project from <c>RegOSDbContext</c>
/// directly with <c>AsNoTracking()</c> — a query handler never loads an
/// aggregate (ADR-016).
/// </summary>
public interface IContactRepository
{
    Task AddAsync(ContactAggregate contact, CancellationToken cancellationToken);

    /// <summary>Loads the contact with its roles, emails and phones.</summary>
    Task<ContactAggregate?> GetByIdAsync(
        ContactId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        ContactAggregate contact,
        CancellationToken cancellationToken);
}
