using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Study.Infrastructure.Repositories;

public sealed class NonClinicalStudyRepository : INonClinicalStudyRepository
{
    private readonly RegOSDbContext _dbContext;

    public NonClinicalStudyRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        NonClinicalStudyAggregate study,
        CancellationToken cancellationToken)
    {
        _dbContext.NonClinicalStudies.Add(study);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<NonClinicalStudyAggregate?> GetByIdAsync(
        NonClinicalStudyId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.NonClinicalStudies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        NonClinicalStudyAggregate study,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
