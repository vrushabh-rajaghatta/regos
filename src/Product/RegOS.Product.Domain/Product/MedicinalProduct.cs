using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// A tenant's product in <em>one</em> regulatory jurisdiction — the tier
/// between the global identity and the licences granted over it.
/// </summary>
/// <remarks>
/// <b>It is identified by its own identity, not by the (global product,
/// country) pair.</b> Several medicinal products may exist for the same pair
/// when the business distinguishes them — different presentations, strengths,
/// or the two halves of a partial divestment — so nothing enforces uniqueness
/// on it, and nothing resolves-or-creates one on a caller's behalf.
/// <para>
/// It exists because a market presence and a licence are different facts. A
/// medicinal product means <em>"we market, or intend to market, this product
/// here"</em>, and can hold that meaning for years with zero registrations:
/// dossier preparation, labelling, artwork, pricing and launch planning all
/// precede authorisation. The dependency runs <c>GlobalProduct →
/// MedicinalProduct → Registration</c>, and only ever that way.
/// </para>
/// <para>
/// Deliberately almost empty. Trade name (S002), market status (S003), ATC code
/// and strength (EPIC-010) and the local label (EPIC-018) all belong here, and
/// each arrives with the feature that reads it. What this story establishes is
/// the identity they hang from.
/// </para>
/// <para>
/// Lives beside <see cref="GlobalProduct"/> rather than in a folder of its own:
/// the folder is named for the context's domain, not for one aggregate, and
/// keeping both tiers in it avoids the namespace-equals-type-name collision
/// that S000 was able to delete fourteen <c>using</c> aliases to remove.
/// </para>
/// </remarks>
public sealed class MedicinalProduct : AggregateRoot<MedicinalProductId>
{
    // Parameterized private constructor, no parameterless one: EF binds by
    // parameter name, and this keeps every field non-nullable. Same shape as
    // GlobalProduct beside it.
    private MedicinalProduct(
        MedicinalProductId id,
        TenantId tenantId,
        GlobalProductId globalProductId,
        CountryId countryId,
        MedicinalProductStatus status,
        DateOnly statusDate)
    {
        Id = id;
        TenantId = tenantId;
        GlobalProductId = globalProductId;
        CountryId = countryId;
        Status = status;
        StatusDate = statusDate;
    }

    /// <summary>
    /// The owning tenant (ADR-031). Set once: moving a market-local record
    /// between tenants would be a transfer, with its own rules.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// The global identity this localises. Immutable — repointing it would
    /// silently rewrite what every licence beneath it authorises.
    /// </summary>
    public GlobalProductId GlobalProductId { get; }

    /// <summary>
    /// The jurisdiction. Immutable for the same reason, and the single
    /// authoritative answer to "which market is this registration for?" —
    /// <c>Registration</c> deliberately does not carry a second copy.
    /// </summary>
    public CountryId CountryId { get; }

    /// <summary>
    /// An activation flag, not the market status — see
    /// <see cref="MedicinalProductStatus"/>. It carries no transitions yet:
    /// the capability to retire a market-local record arrives with S003, where
    /// the market lifecycle makes it something a user can actually reach.
    /// </summary>
    public MedicinalProductStatus Status { get; private set; }

    public DateOnly StatusDate { get; private set; }

    /// <param name="statusDate">
    /// The business date this market-local record came into being — supplied,
    /// never read from the clock, so a migrated portfolio can state when the
    /// market presence actually began.
    /// </param>
    public static MedicinalProduct Create(
        TenantId tenantId,
        GlobalProductId globalProductId,
        CountryId countryId,
        DateOnly statusDate)
    {
        if (tenantId is null)
            throw new DomainException(MedicinalProductErrors.TenantRequired);

        if (globalProductId is null)
            throw new DomainException(
                MedicinalProductErrors.GlobalProductRequired);

        if (countryId == default)
            throw new DomainException(MedicinalProductErrors.CountryRequired);

        if (statusDate == default)
            throw new DomainException(
                MedicinalProductErrors.StatusDateRequired);

        return new MedicinalProduct(
            MedicinalProductId.New(),
            tenantId,
            globalProductId,
            countryId,
            MedicinalProductStatus.Active,
            statusDate);
    }
}
