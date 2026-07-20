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
/// The identifier is a bare <see cref="Guid"/> rather than a strongly typed id
/// on purpose. <c>OrganizationId</c> belongs to a bounded context, and the
/// shared kernel must not depend on one; and while a tenant happens to be an
/// organization today, nothing guarantees that stays true. Each bounded context
/// converts at its own boundary — <c>new OrganizationId(tenantContext.TenantId)</c>
/// — which is a small seam in exchange for keeping the dependency direction
/// clean and avoiding a speculative <c>TenantId</c> type.
/// </para>
/// </remarks>
public interface ITenantContext
{
    /// <summary>
    /// The tenant the current request is acting on behalf of. Implementations
    /// must never return an empty guid: if the tenant cannot be determined they
    /// throw, so that a missing tenant can never silently widen a query.
    /// </summary>
    Guid TenantId { get; }
}
