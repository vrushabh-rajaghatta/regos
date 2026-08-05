using RegOS.Product.Domain.Product;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Registration.Domain.Aggregates.PackAuthorisations;

/// <summary>
/// A licence authorising one pack, from a date.
/// </summary>
/// <remarks>
/// <b>A dated relationship, not a foreign key</b>
/// (<see href="../../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>
/// §3). RIM says <c>License → Packaged Product</c>, <em>Single</em>, and that is
/// wrong twice over: one licence routinely authorises a family of packs, and
/// packs frequently arrive years after the original authorisation, by variation.
/// A foreign key on either aggregate can express neither.
/// <para>
/// <b>It lives in Registration because only Registration can name both
/// types.</b> <c>Registration.Domain</c> already references
/// <c>Product.Domain</c>, so a <c>RegistrationId</c> on the pack would close a
/// dependency cycle — found by the compiler while writing
/// <see cref="PackagedProduct"/>, not by review. The constraint improved the
/// model: <see cref="Registration"/> itself is untouched, <c>Product</c> stays
/// independent of who authorised anything, and the relationship gained a date it
/// could not otherwise carry.
/// </para>
/// <para>
/// <b>Its own root rather than a child of <see cref="Registration"/>.</b> The
/// question asked of it — <em>"which packs are authorised in this market?"</em>
/// — starts at the market and reaches licences, not the other way round; making
/// it a child would mean loading every registration to answer it. It is also
/// what keeps <see cref="Registration"/> genuinely unchanged, which was the
/// point.
/// </para>
/// <para>
/// <b>Somewhere to grow.</b> <em>Which variation authorised this pack? Which
/// submission introduced it? Which sequence first approved it?</em> are all
/// properties of the authorisation event and none of them properties of the
/// pack. None is built until one is asked for.
/// </para>
/// </remarks>
public sealed class PackAuthorisation : AggregateRoot<PackAuthorisationId>
{
    private PackAuthorisation()
    {
    }

    /// <summary>The owning tenant (ADR-031). Fail-closed, set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>The licence that authorises the pack.</summary>
    public RegistrationId RegistrationId { get; private set; }

    /// <summary>The pack it authorises.</summary>
    public PackagedProductId PackagedProductId { get; private set; } = default!;

    /// <summary>
    /// The business date the pack became authorised under this licence.
    /// </summary>
    /// <remarks>
    /// <b>The fact a foreign key could not carry, and the reason this type
    /// exists at all.</b> A licence granted in 2021 that gained its 100-pack in
    /// 2024 has two dates, and only one of them is the registration's.
    /// </remarks>
    public DateOnly AuthorisedOn { get; private set; }

    public DateTime RecordedOnUtc { get; private set; }

    public static PackAuthorisation Create(
        TenantId tenantId,
        RegistrationId registrationId,
        PackagedProductId packagedProductId,
        DateOnly authorisedOn)
    {
        if (tenantId is null)
            throw new DomainException(PackAuthorisationErrors.TenantRequired);

        if (packagedProductId is null)
            throw new DomainException(PackAuthorisationErrors.PackRequired);

        if (authorisedOn == default)
            throw new DomainException(
                PackAuthorisationErrors.AuthorisedOnRequired);

        return new PackAuthorisation
        {
            Id = PackAuthorisationId.New(),
            TenantId = tenantId,
            RegistrationId = registrationId,
            PackagedProductId = packagedProductId,
            AuthorisedOn = authorisedOn,
            RecordedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Corrects the date the pack was authorised.
    /// </summary>
    /// <remarks>
    /// The pair it names is immutable: a different pack or a different licence
    /// is a different authorisation, and editing one into another would leave no
    /// way to tell a correction from a replacement — the same call
    /// <c>RestateIngredient</c> makes about its substance.
    /// </remarks>
    public void Correct(DateOnly authorisedOn)
    {
        if (authorisedOn == default)
            throw new DomainException(
                PackAuthorisationErrors.AuthorisedOnRequired);

        AuthorisedOn = authorisedOn;
    }
}
