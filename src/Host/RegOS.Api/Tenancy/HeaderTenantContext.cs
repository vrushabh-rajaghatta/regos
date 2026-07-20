using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Tenancy;

/// <summary>
/// Resolves the tenant from the <c>X-Tenant-Id</c> request header.
/// </summary>
/// <remarks>
/// <para>
/// <b>Header-based tenant resolution provides deterministic request scoping for
/// development. It is not an authentication or authorization mechanism.</b> Any
/// caller can set this header to any value, so it establishes <em>which</em>
/// tenant a request is scoped to, never that the caller is entitled to it. When
/// authentication arrives, this implementation is replaced by one reading a
/// claim; nothing above it changes, which is the point of the abstraction.
/// </para>
/// <para>
/// Resolution is deliberately lazy — the header is read when
/// <see cref="TenantId"/> is first accessed, not in the constructor. Endpoints
/// that are not tenant-scoped (reference data, master data) never touch the
/// property and so are never forced to supply a header they have no use for.
/// </para>
/// <para>
/// A missing or malformed header throws rather than returning a default. That
/// is the whole safety property of this story: there is no value of the header
/// that yields an unscoped query, so tenant isolation cannot be switched off by
/// omission.
/// </para>
/// </remarks>
public sealed class HeaderTenantContext : ITenantContext
{
    public const string HeaderName = "X-Tenant-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext
                ?? throw new DomainException(TenantErrors.Unavailable);

            if (!context.Request.Headers.TryGetValue(HeaderName, out var values)
                || values.Count == 0
                || string.IsNullOrWhiteSpace(values[0]))
            {
                throw new DomainException(TenantErrors.Missing);
            }

            if (!Guid.TryParse(values[0], out var tenantId)
                || tenantId == Guid.Empty)
            {
                throw new DomainException(TenantErrors.Malformed);
            }

            return tenantId;
        }
    }
}
