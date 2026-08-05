using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.Product.Domain.Product;

public sealed class TradeNameId : StronglyTypedId
{
    public TradeNameId(Guid value) : base(value)
    {
    }

    public static TradeNameId New() => new(Guid.NewGuid());
}

/// <summary>
/// What a product is called in one market, in one language — the brand a
/// patient or prescriber would recognise, as the pair <em>language + name</em>.
/// </summary>
/// <remarks>
/// A child entity of <see cref="MedicinalProduct"/>, not a root: nobody quotes
/// its id, it has no lifecycle of its own, and no query reaches it directly.
/// <em>"What is this product called in Canada?"</em> enters through the market
/// and stops there. It therefore carries no <c>TenantId</c> — it is reachable
/// only through a filtered root (ADR-031).
/// <para>
/// The country is <b>inherited from the parent</b> rather than repeated here.
/// RIM carries country on Trade Name; the tier makes that unnecessary, and is
/// one of the reasons the tier is worth having.
/// </para>
/// <para>
/// <b>Not an instance of scheme-plus-value.</b> This is (language, name), and
/// <c>SiteIdentifier</c>/<c>OrganizationIdentifier</c> are (scheme, value) with
/// a different rule attached. EPIC-017 was named as the likely third consumer
/// of that shape; it is not, and both breadcrumbs stay standing. The Rule of
/// Three (ADR-018) fires when an abstraction emerges, not when two classes
/// happen to hold two properties.
/// </para>
/// </remarks>
public sealed class TradeName : Entity<TradeNameId>
{
    public const int NameMaxLength = 200;

    // Only the aggregate creates these.
    internal TradeName(TradeNameId id, LanguageCode language, string? name)
    {
        Id = id;
        Language = language;
        Name = Normalize(name);
    }

    public LanguageCode Language { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    private static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(MedicinalProductErrors.TradeNameRequired);

        var trimmed = name.Trim();

        return trimmed.Length > NameMaxLength
            ? throw new DomainException(MedicinalProductErrors.TradeNameTooLong)
            : trimmed;
    }
}
