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

    // ListByProductAsync used to sit here. It had no callers — the portfolio
    // views are query handlers reading the DbContext directly, as ADR-016
    // requires — and re-pointing it to the medicinal product would have been
    // maintaining a method for nobody. Removed with the re-pointing that broke
    // it rather than kept alive by it.
}
