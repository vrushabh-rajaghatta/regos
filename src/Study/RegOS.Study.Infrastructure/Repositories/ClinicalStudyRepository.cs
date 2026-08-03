using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;

namespace RegOS.Study.Infrastructure.Repositories;

public sealed class ClinicalStudyRepository : IClinicalStudyRepository
{
    private readonly RegOSDbContext _dbContext;

    public ClinicalStudyRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ClinicalStudyAggregate study,
        CancellationToken cancellationToken)
    {
        _dbContext.ClinicalStudies.Add(study);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ClinicalStudyAggregate?> GetByIdAsync(
        ClinicalStudyId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ClinicalStudies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        ClinicalStudyAggregate study,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
