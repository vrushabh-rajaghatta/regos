using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.Persistence;

namespace RegOS.Labeling.Infrastructure.Repositories;

public sealed class ContraindicationRepository : IContraindicationRepository
{
    private readonly RegOSDbContext _dbContext;

    public ContraindicationRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Contraindication statement, CancellationToken cancellationToken)
    {
        await _dbContext.Contraindications.AddAsync(statement, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with populations.
    /// </summary>
    /// <remarks>
    /// No <c>Include</c>: populations are an owned collection, and EF loads an
    /// owner's owned types with it. That is a consequence of the mapping S004
    /// chose, not an omission — and it is why the rules that read the collection
    /// cannot silently see an empty one.
    /// </remarks>
    public async Task<Contraindication?> GetByIdAsync(
        ContraindicationId id,
        CancellationToken cancellationToken)
        => await _dbContext.Contraindications
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        Contraindication statement,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
