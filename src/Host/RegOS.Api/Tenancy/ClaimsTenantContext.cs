using RegOS.Platform.Application.Services;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Api.Tenancy;

/// <summary>
/// Resolves the tenant from the authenticated caller's tenant claim.
/// </summary>
/// <remarks>
/// <para>
/// This is the whole of it. There is no token parsing, no header reading and no
/// fallback, because <see cref="ICurrentUser"/> has already been populated from
/// a signature-checked token. A tenant is now something the caller
/// <em>proved</em> rather than something they asserted.
/// </para>
/// <para>
/// It replaces <c>HeaderTenantContext</c>, which read <c>X-Tenant-Id</c> and
/// therefore let any caller name any tenant. That was always documented as a
/// development mechanism; this is the slice where the documentation stops being
/// a promise (ADR-024).
/// </para>
/// <para>
/// Still lazy, for the same reason as before: endpoints that are not
/// tenant-scoped never read the property, so they are not forced into
/// authentication they have no use for.
/// </para>
/// </remarks>
public sealed class ClaimsTenantContext : ITenantContext
{
    private readonly ICurrentUser _currentUser;

    public ClaimsTenantContext(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    // Unauthenticated access throws from ICurrentUser as a 401. That is a
    // stricter failure than the old 400: asking for tenant-scoped data without
    // proving who you are is now a question about identity, not about a
    // malformed request.
    public TenantId TenantId => _currentUser.TenantId;

    // The lenient sibling, for the global query filters only: they evaluate on
    // requests that have no identity yet (login, refresh, password reset) and
    // must resolve to *no rows* there rather than throw. Routed through
    // ICurrentUser so there is exactly one place that reads the claim; the
    // catch covers a caller who is authenticated but carries no tenant, which
    // a platform user will be.
    public TenantId? TenantIdOrNull
    {
        get
        {
            if (!_currentUser.IsAuthenticated)
                return null;

            try
            {
                return _currentUser.TenantId;
            }
            catch (AuthenticationFailedException)
            {
                return null;
            }
        }
    }
}
