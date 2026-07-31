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
            .Include(x => x.History)
            .FirstOrDefaultAsync(x => x.Id == registrationId, cancellationToken);

        if (registration is null)
            return null;

        // The aggregate carries ids; names come from the referenced records.
        // Name is a value object; project the string it wraps.
        var product = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == registration.GlobalProductId)
            .Select(x => x.Name.Value)
            .FirstOrDefaultAsync(cancellationToken);

        var country = await _dbContext.Countries
            .AsNoTracking()
            .Where(x => x.Id == registration.CountryId)
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
            registration.GlobalProductId.Value,
            product ?? string.Empty,
            registration.CountryId.Value,
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
                // Chronological by what happened, then by what was learned:
                // two entries can share a business date when a portfolio is
                // migrated, and the order they were recorded in is the
                // tie-break a reader expects.
                .OrderBy(entry => entry.OccurredOn)
                .ThenBy(entry => entry.RecordedOnUtc)
                .Select(entry => new RegistrationStatusEntryDto(
                    entry.Id.Value,
                    entry.Status.ToString(),
                    entry.OccurredOn,
                    entry.RecordedOnUtc,
                    entry.Note))],
            // Asked of the domain, never restated here: one table decides what
            // is legal, and the read model reports its answer.
            [.. RegistrationLifecycle.From(registration.CurrentStatus)
                .OrderBy(status => status)
                .Select(status => status.ToString())],
            expiry.HasRunningValidity,
            expiry.DaysUntilExpiry,
            expiry.IsExpired);
    }
}
