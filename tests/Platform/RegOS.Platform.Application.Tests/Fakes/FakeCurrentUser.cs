using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

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
    public FakeCurrentUser(UserId userId, OrganizationId organizationId, Email email)
    {
        UserId = userId;
        OrganizationId = organizationId;
        Email = email;
    }

    public bool IsAuthenticated => true;

    public UserId UserId { get; }

    public OrganizationId OrganizationId { get; }

    public Email Email { get; }
}
