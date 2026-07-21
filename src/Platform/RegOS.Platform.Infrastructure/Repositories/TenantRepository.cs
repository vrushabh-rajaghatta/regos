using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Infrastructure.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly RegOSDbContext _dbContext;

    public TenantRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tenant?> GetByIdAsync(
        TenantId id,
        CancellationToken cancellationToken)
    {
        // Tenants carry no query filter — the directory is global by
        // definition (ADR-031) — so no bypass is needed here.
        return await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        Tenant tenant,
        CancellationToken cancellationToken)
    {
        _dbContext.Tenants.Update(tenant);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
