using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Commitments;
using RegOS.Persistence;

namespace RegOS.Interaction.Infrastructure.Repositories;

public sealed class CommitmentRepository : ICommitmentRepository
{
    private readonly RegOSDbContext _dbContext;

    public CommitmentRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Commitment commitment, CancellationToken cancellationToken)
    {
        await _dbContext.Commitments.AddAsync(commitment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Commitment?> GetByIdAsync(
        CommitmentId id,
        CancellationToken cancellationToken)
        => await _dbContext.Commitments
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(Commitment commitment, CancellationToken cancellationToken)
    {
        _dbContext.Commitments.Update(commitment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
