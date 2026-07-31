using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Infrastructure.Services;

public sealed class MedicinalProductPolicy : IMedicinalProductPolicy
{
    private readonly RegOSDbContext _dbContext;

    public MedicinalProductPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureCanCreateAsync(
        GlobalProductId globalProductId,
        CountryId countryId,
        CancellationToken cancellationToken)
    {
        // The global product is ADDRESSED by the route
        // (POST /api/products/{globalProductId}/medicinal-products), so its
        // absence is a 404. The country is a *referenced* value and stays 400 —
        // the same split RegistrationCreationPolicy makes.
        var productExists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == globalProductId, cancellationToken);

        if (!productExists)
            throw new NotFoundException(
                MedicinalProductPolicyErrors.GlobalProductDoesNotExist);

        var countryExists = await _dbContext.Countries
            .AsNoTracking()
            .AnyAsync(x => x.Id == countryId, cancellationToken);

        if (!countryExists)
            throw new DomainException(
                MedicinalProductPolicyErrors.CountryDoesNotExist);
    }
}
