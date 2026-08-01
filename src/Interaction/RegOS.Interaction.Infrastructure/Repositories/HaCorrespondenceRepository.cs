using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Correspondence;
using RegOS.Persistence;

namespace RegOS.Interaction.Infrastructure.Repositories;

public sealed class HaCorrespondenceRepository : IHaCorrespondenceRepository
{
    private readonly RegOSDbContext _dbContext;

    public HaCorrespondenceRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        HaCorrespondence correspondence,
        CancellationToken cancellationToken)
    {
        await _dbContext.HaCorrespondence.AddAsync(correspondence, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<HaCorrespondence?> GetByIdAsync(
        HaCorrespondenceId id,
        CancellationToken cancellationToken)
        => await _dbContext.HaCorrespondence
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        HaCorrespondence correspondence,
        CancellationToken cancellationToken)
    {
        _dbContext.HaCorrespondence.Update(correspondence);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
