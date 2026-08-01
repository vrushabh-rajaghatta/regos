using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.Platform.Contracts;

using UserCredentialAggregate =
    RegOS.Platform.Domain.Aggregates.UserCredential.UserCredential;

namespace RegOS.Platform.Infrastructure.Repositories;

public sealed class UserCredentialRepository : IUserCredentialRepository
{
    private readonly RegOSDbContext _dbContext;

    public UserCredentialRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        UserCredentialAggregate credential,
        CancellationToken cancellationToken)
    {
        await _dbContext.UserCredentials.AddAsync(credential, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserCredentialAggregate?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken)
        => await _dbContext.UserCredentials
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public async Task UpdateAsync(
        UserCredentialAggregate credential,
        CancellationToken cancellationToken)
    {
        _dbContext.UserCredentials.Update(credential);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
