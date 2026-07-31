using RegOS.Product.Domain.Product;

namespace RegOS.Registration.Domain.Aggregates.Registration;

public interface IRegistrationRepository
{
    Task AddAsync(
        Registration registration,
        CancellationToken cancellationToken);

    /// <summary>Tracked, with history — for mutation.</summary>
    Task<Registration?> GetByIdAsync(
        RegistrationId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Registration registration,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Registration>> ListByProductAsync(
        GlobalProductId globalProductId,
        CancellationToken cancellationToken);
}
