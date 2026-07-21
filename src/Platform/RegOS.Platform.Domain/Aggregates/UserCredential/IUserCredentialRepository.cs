using RegOS.Platform.Domain.Aggregates.User;

using UserCredentialAggregate =
    RegOS.Platform.Domain.Aggregates.UserCredential.UserCredential;

namespace RegOS.Platform.Domain.Aggregates.UserCredential;

/// <summary>
/// Interface in the domain, implementation in infrastructure — matching
/// <see cref="IUserRepository"/>, the convention this bounded context uses.
/// </summary>
public interface IUserCredentialRepository
{
    Task AddAsync(
        UserCredentialAggregate credential,
        CancellationToken cancellationToken);

    Task<UserCredentialAggregate?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        UserCredentialAggregate credential,
        CancellationToken cancellationToken);
}
