using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Primitives;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>
/// The caller behind the access token.
/// </summary>
/// <remarks>
/// The one part of a change-password test that must be faked: the real
/// implementation reads claims off an <c>HttpContext</c>, which is the host's
/// concern and is exercised by the integration tests. Everything else in these
/// tests is real.
/// </remarks>
public sealed class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(
        UserId userId,
        TenantId tenantId,
        Email email,
        UserRole role = UserRole.Member)
    {
        UserId = userId;
        TenantId = tenantId;
        Email = email;
        Role = role;
    }

    public bool IsAuthenticated => true;

    public UserId UserId { get; }

    public TenantId TenantId { get; }

    public Email Email { get; }

    public UserRole Role { get; }
}
