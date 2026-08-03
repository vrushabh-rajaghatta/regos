using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Infrastructure.Repositories;

/// <inheritdoc cref="ISubstanceRepository"/>
public sealed class SubstanceRepository : ISubstanceRepository
{
    private readonly RegOSDbContext _dbContext;

    public SubstanceRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Substance substance, CancellationToken cancellationToken)
    {
        _dbContext.Substances.Add(substance);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Substance?> FindVisibleByNameAsync(
        TenantId tenantId,
        string name,
        CancellationToken cancellationToken)
    {
        // Case-insensitive equality, not ILike: a name is being compared for
        // sameness rather than searched, and ILike would read a compound whose
        // name contains "%" or "_" as a wildcard and refuse a name nobody has
        // taken.
        var normalized = name.ToLowerInvariant();

        // The shared-plus-extensible filter is in the predicate as well as on
        // the model (ADR-031). Redundant, and deliberately so: this is the rule
        // whose correctness a reader should be able to see without knowing the
        // query filter is there.
        return await _dbContext.Substances
            .AsNoTracking()
            .Where(x => x.TenantId == null || x.TenantId == tenantId)
            .FirstOrDefaultAsync(
                x => x.Name.ToLower() == normalized, cancellationToken);
    }
}
