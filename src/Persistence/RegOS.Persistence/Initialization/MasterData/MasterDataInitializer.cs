using Microsoft.EntityFrameworkCore;

using RegOS.Persistence.Initialization.MasterData.Geography;
using RegOS.Persistence.Initialization.MasterData.Regulatory;

namespace RegOS.Persistence.Initialization.MasterData;

public sealed class MasterDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public MasterDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Countries.AnyAsync(cancellationToken))
        {
            _dbContext.Countries.AddRange(Countries.Data);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await _dbContext.Authorities.AnyAsync(cancellationToken))
        {
            _dbContext.Authorities.AddRange(Authorities.Data);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
