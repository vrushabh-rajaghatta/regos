using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.Substances;

/// <summary>
/// <b>Add, and ask whether a name is taken.</b> Nothing loads a substance for
/// mutation and nothing saves a change, because S001 ships no way to modify
/// one — which is how a shared row refuses mutation without a guard standing
/// over a capability that does not exist (ADR-058 §5).
/// </summary>
/// <remarks>
/// Stewardship, lifecycle and change control are EPIC-012's, and each will add
/// the method it needs. This interface is deliberately not shaped in advance of
/// them.
/// </remarks>
public interface ISubstanceRepository
{
    Task AddAsync(Substance substance, CancellationToken cancellationToken);

    /// <summary>
    /// Whether <paramref name="name"/> already names a substance this tenant
    /// can see — either in the shared catalogue or among their own.
    /// </summary>
    /// <remarks>
    /// <b>A unique index cannot express this.</b> Uniqueness is per tenant, so
    /// two tenants may each add <c>Compound-X</c>; but a tenant adding a name
    /// the shared catalogue already carries would fork the answer to <em>"which
    /// products contain substance X?"</em> on the first screen of the epic that
    /// exists to answer it. The index covers the half it can and this covers
    /// the half it cannot — the same division <c>SponsorStudyIdentifierPolicy</c>
    /// draws.
    /// </remarks>
    Task<Substance?> FindVisibleByNameAsync(
        TenantId tenantId,
        string name,
        CancellationToken cancellationToken);
}
