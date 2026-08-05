namespace RegOS.Registration.Domain.Aggregates.PackAuthorisations;

public interface IPackAuthorisationRepository
{
    Task AddAsync(
        PackAuthorisation authorisation,
        CancellationToken cancellationToken);

    /// <summary>Tracked — for mutation.</summary>
    Task<PackAuthorisation?> GetByIdAsync(
        PackAuthorisationId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        PackAuthorisation authorisation,
        CancellationToken cancellationToken);

    Task RemoveAsync(
        PackAuthorisation authorisation,
        CancellationToken cancellationToken);
}
