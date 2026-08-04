using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.Persistence;

namespace RegOS.Labeling.Infrastructure.Repositories;

public sealed class DrugInteractionRepository : IDrugInteractionRepository
{
    private readonly RegOSDbContext _dbContext;

    public DrugInteractionRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        DrugInteraction interaction,
        CancellationToken cancellationToken)
    {
        await _dbContext.Interactions.AddAsync(interaction, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with interactants and populations.
    /// </summary>
    /// <remarks>
    /// No <c>Include</c>: both are owned collections, so EF loads them with the
    /// owner. That matters more here than anywhere else in the context — the
    /// at-least-one rule counts interactants, and against an unloaded collection
    /// it would happily remove the last one.
    /// </remarks>
    public async Task<DrugInteraction?> GetByIdAsync(
        DrugInteractionId id,
        CancellationToken cancellationToken)
        => await _dbContext.Interactions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        DrugInteraction interaction,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
