using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Registration.Domain.Aggregates.Registration;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Application.Queries.GetRegistration;

public sealed class GetRegistrationHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetRegistrationHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the registration with the names a person reads, or null when it
    /// does not exist — so the endpoint can 404 rather than return an empty one.
    /// </summary>
    public async Task<RegistrationDetailDto?> HandleAsync(
        RegistrationId registrationId,
        CancellationToken cancellationToken)
    {
        var registration = await _dbContext.Set<RegistrationAggregate>()
            .AsNoTracking()
            // Deterministic: an entry id is unique, so this is a total order —
            // and it is an ORDERED include on purpose. EF applies it in SQL,
            // where an id translates; the in-memory sort below then only has to
            // be stable, which LINQ-to-Objects guarantees (BUG-001).
            .Include(x => x.History.OrderBy(entry => entry.Id))
            .FirstOrDefaultAsync(x => x.Id == registrationId, cancellationToken);

        if (registration is null)
            return null;

        // The aggregate carries one id upwards; the global product and the
        // market are read from the tier it names.
        var market = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .Where(x => x.Id == registration.MedicinalProductId)
            .Select(x => new { x.GlobalProductId, x.CountryId })
            .FirstOrDefaultAsync(cancellationToken);

        // Names come from the referenced records. Name is a value object;
        // project the string it wraps.
        var product = market is null
            ? null
            : await _dbContext.Products
                .AsNoTracking()
                .Where(x => x.Id == market.GlobalProductId)
                .Select(x => x.Name.Value)
                .FirstOrDefaultAsync(cancellationToken);

        var country = market is null
            ? null
            : await _dbContext.Countries
                .AsNoTracking()
                .Where(x => x.Id == market.CountryId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);

        var authority = await _dbContext.Authorities
            .AsNoTracking()
            .Where(x => x.Id == registration.AuthorityId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var holder = await _dbContext.Organizations
            .AsNoTracking()
            .Where(x => x.Id == registration.HolderOrganizationId)
            .Select(x => x.LegalName)
            .FirstOrDefaultAsync(cancellationToken);

        var expiry = ExpiryVisibility.For(
            registration.CurrentStatus,
            registration.ExpiresOn,
            ExpiryVisibility.Today());

        return new RegistrationDetailDto(
            registration.Id.Value,
            registration.MedicinalProductId.Value,
            market?.GlobalProductId.Value ?? Guid.Empty,
            product ?? string.Empty,
            market?.CountryId.Value ?? Guid.Empty,
            country ?? string.Empty,
            registration.AuthorityId.Value,
            authority ?? string.Empty,
            registration.HolderOrganizationId.Value,
            holder ?? string.Empty,
            registration.OriginatingApplicationId?.Value,
            registration.RegistrationNumber,
            registration.CurrentStatus.ToString(),
            registration.ApprovedOn,
            registration.ExpiresOn,
            registration.CreatedOn,
            [.. registration.History
                // Deterministic: the Include above orders the history by entry
                // id in SQL and this sort is stable (BUG-001).
                //
                // Chronological by what happened, then by what was learned:
                // two entries can share a business date when a portfolio is
                // migrated, and the order they were recorded in is the
                // tie-break a reader expects.
                .OrderBy(entry => entry.OccurredOn)
                .ThenBy(entry => entry.RecordedOnUtc)
                // BUG-001: no id tie-break here — the Include above orders the
                // history in SQL and this sort is stable.
                .Select(entry => new RegistrationStatusEntryDto(
                    entry.Id.Value,
                    entry.Status.ToString(),
                    entry.OccurredOn,
                    entry.RecordedOnUtc,
                    entry.Note))],
            // Asked of the domain, never restated here: one table decides what
            // is legal, and the read model reports its answer.
            [.. RegistrationLifecycle.From(registration.CurrentStatus)
                // Deterministic: a lifecycle returns distinct enum values, so
                // sorting them by their own value cannot tie.
                // Deterministic: a lifecycle returns distinct enum values, so
                // sorting them by their own value cannot tie.
                .OrderBy(status => status)
                .Select(status => status.ToString())],
            expiry.HasRunningValidity,
            expiry.DaysUntilExpiry,
            expiry.IsExpired);
    }
}
