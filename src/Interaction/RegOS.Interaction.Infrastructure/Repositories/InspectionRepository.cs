using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Inspections;
using RegOS.Persistence;

namespace RegOS.Interaction.Infrastructure.Repositories;

public sealed class InspectionRepository : IInspectionRepository
{
    private readonly RegOSDbContext _dbContext;

    public InspectionRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Inspection inspection, CancellationToken cancellationToken)
    {
        await _dbContext.Inspections.AddAsync(inspection, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Inspection?> GetByIdAsync(
        InspectionId id, CancellationToken cancellationToken)
        => await _dbContext.Inspections
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(Inspection inspection, CancellationToken cancellationToken)
    {
        _dbContext.Inspections.Update(inspection);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
