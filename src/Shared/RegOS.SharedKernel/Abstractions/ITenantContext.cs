using RegOS.SharedKernel.Primitives;

namespace RegOS.SharedKernel.Abstractions;

/// <summary>
/// Answers one question for the current request: <em>who is asking?</em>
/// </summary>
/// <remarks>
/// <para>
/// Tenant is not the same concept as Organization, and the distinction is
/// deliberate:
/// </para>
/// <list type="bullet">
///   <item><b>Tenant</b> — infrastructure. Ambient, resolved from the request,
///   and never a property of a command or query. It says who the caller is.</item>
///   <item><b>Organization</b> — domain. Explicit wherever it is part of the
///   ubiquitous language, such as the applicant on a regulatory application.
///   It says something about the record.</item>
/// </list>
/// <para>
/// The identifier is a <see cref="Primitives.TenantId"/> owned by the kernel
/// itself. It used to be a bare <see cref="Guid"/> so the kernel would not
/// depend on the Organization context's <c>OrganizationId</c>; now that the
/// tenant has its own id type here, each bounded context uses it directly and
/// the per-boundary conversion seam is gone (ADR-030).
/// </para>
/// </remarks>
public interface ITenantContext
{
    /// <summary>
    /// The tenant the current request is acting on behalf of. Implementations
    /// must never return an empty id: if the tenant cannot be determined they
    /// throw, so that a missing tenant can never silently widen a query.
    /// </summary>
    TenantId TenantId { get; }

    /// <summary>
    /// The tenant if one can be determined, otherwise <c>null</c>. This exists
    /// for exactly one consumer: the global query filters, which must evaluate
    /// on requests that have no identity yet (login, refresh, password reset)
    /// and must resolve to <em>no rows</em> there rather than throw. Handlers
    /// use <see cref="TenantId"/>, which fails loudly instead.
    /// </summary>
    TenantId? TenantIdOrNull { get; }
}
