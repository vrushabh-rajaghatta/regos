using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.Organization;

public sealed class OrganizationInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public OrganizationInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Organizations.AnyAsync(cancellationToken))
            return;

        _dbContext.Organizations.AddRange(Organizations.Data);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
