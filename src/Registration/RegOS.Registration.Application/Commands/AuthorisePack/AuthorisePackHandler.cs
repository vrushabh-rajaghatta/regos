using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.Registration.Domain.Aggregates.PackAuthorisations;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Registration.Application.Commands.AuthorisePack;

public sealed class AuthorisePackHandler
{
    private readonly IPackAuthorisationRepository _authorisations;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenant;

    public AuthorisePackHandler(
        IPackAuthorisationRepository authorisations,
        RegOSDbContext dbContext,
        ITenantContext tenant)
    {
        _authorisations = authorisations;
        _dbContext = dbContext;
        _tenant = tenant;
    }

    public async Task<AuthorisePackResult> HandleAsync(
        AuthorisePackCommand command,
        CancellationToken cancellationToken)
    {
        // Both reads go through the fail-closed filters, so another tenant's
        // licence or pack is not found rather than refused (ADR-031).
        var registration = await _dbContext.Registrations
            .AsNoTracking()
            .Where(x => x.Id == command.RegistrationId)
            .Select(x => new { x.MedicinalProductId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                PackAuthorisationErrors.RegistrationDoesNotExist);

        var pack = await _dbContext.PackagedProducts
            .AsNoTracking()
            .Where(x => x.Id == command.PackagedProductId)
            .Select(x => new { x.MedicinalProductId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(PackagedProductErrors.NotFound);

        // A UK licence authorising a French pack has two real rows, both the
        // tenant's, and nothing else would notice. The registration names a
        // medicinal product and so does the pack, so the market is the one
        // fact they must agree on.
        if (pack.MedicinalProductId != registration.MedicinalProductId)
            throw new BusinessRuleViolationException(
                PackAuthorisationErrors.PackBelongsToAnotherMarket);

        var alreadySaid = await _dbContext.PackAuthorisations
            .AsNoTracking()
            .AnyAsync(
                x => x.RegistrationId == command.RegistrationId
                    && x.PackagedProductId == command.PackagedProductId,
                cancellationToken);

        // The unique index says the same thing where a race cannot slip past
        // this check; here so the refusal names the act rather than a constraint.
        if (alreadySaid)
            throw new BusinessRuleViolationException(
                PackAuthorisationErrors.AlreadyAuthorised);

        var authorisation = PackAuthorisation.Create(
            _tenant.TenantId,
            command.RegistrationId,
            command.PackagedProductId,
            command.AuthorisedOn);

        await _authorisations.AddAsync(authorisation, cancellationToken);

        return new AuthorisePackResult(authorisation.Id.Value);
    }
}
