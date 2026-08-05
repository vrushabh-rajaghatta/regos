using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Registration.Domain.Aggregates.PackAuthorisations;

namespace RegOS.Registration.Infrastructure.Repositories;

public sealed class PackAuthorisationRepository : IPackAuthorisationRepository
{
    private readonly RegOSDbContext _dbContext;

    public PackAuthorisationRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        PackAuthorisation authorisation,
        CancellationToken cancellationToken)
    {
        _dbContext.PackAuthorisations.Add(authorisation);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <remarks>
    /// No <c>Include</c>: the aggregate holds two ids and a date, and reasons
    /// across nothing. It is the smallest root in RegOS, which is the shape a
    /// relationship should have.
    /// </remarks>
    public async Task<PackAuthorisation?> GetByIdAsync(
        PackAuthorisationId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.PackAuthorisations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        PackAuthorisation authorisation,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <remarks>
    /// <b>Removed, not deactivated</b>, and this is the exception ES-018 leaves
    /// room for. An authorisation recorded against the wrong licence is a
    /// mistake about a relationship, not an event that happened — there is no
    /// regulatory record to retain, because nothing was ever true. Withdrawing
    /// a real authorisation is a different act, and when somebody asks for it,
    /// it is a status on this row rather than a delete.
    /// </remarks>
    public async Task RemoveAsync(
        PackAuthorisation authorisation,
        CancellationToken cancellationToken)
    {
        _dbContext.PackAuthorisations.Remove(authorisation);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
