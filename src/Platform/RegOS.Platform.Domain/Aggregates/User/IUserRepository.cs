namespace RegOS.Platform.Domain.Aggregates.User;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves a user from their email alone, with no organization. This is
    /// only possible because an email identifies exactly one user across RegOS
    /// (ADR-021), and it is what sign-in needs: authentication runs before any
    /// tenant exists, since the token is what establishes one.
    /// </summary>
    Task<User?> GetByEmailAsync(
        ValueObjects.Email email,
        CancellationToken cancellationToken);

    Task UpdateAsync(User user, CancellationToken cancellationToken);
}
