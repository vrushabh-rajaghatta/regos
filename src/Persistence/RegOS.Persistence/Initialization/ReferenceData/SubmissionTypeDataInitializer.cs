using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.ReferenceData;

public sealed class SubmissionTypeDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public SubmissionTypeDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.SubmissionTypes.AnyAsync(cancellationToken))
        {
            _dbContext.SubmissionTypes.AddRange(SubmissionTypes.Data);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
