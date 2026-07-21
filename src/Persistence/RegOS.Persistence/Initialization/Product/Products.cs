using RegOS.Persistence.Initialization.Platform;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Primitives;

using ProductAggregate = RegOS.Product.Domain.Product.Product;

namespace RegOS.Persistence.Initialization.Product;

/// <summary>
/// Development fixtures. Every product is owned by a seeded tenant, so
/// the environment exercises tenant isolation rather than assuming a single
/// tenant: two different tenants each hold products, and a tenant with none is
/// equally useful for testing empty states.
/// </summary>
internal static class Products
{
    private static readonly TenantId Manufacturer =
        new(TenantIds.DemoManufacturer);

    private static readonly TenantId MarketingAuthorizationHolder =
        new(TenantIds.DemoMarketingAuthorizationHolder);

    public static IReadOnlyList<ProductAggregate> Data { get; } =
    [
        ProductAggregate.Register(Manufacturer, "ACE-500", "Acetaminophen 500mg", ProductType.Drug),
        ProductAggregate.Register(Manufacturer, "IBU-200", "Ibuprofen 200mg", ProductType.Drug),
        ProductAggregate.Register(Manufacturer, "NAP-250", "Naproxen 250mg", ProductType.Drug),
        ProductAggregate.Register(MarketingAuthorizationHolder, "ASP-75", "Aspirin 75mg", ProductType.Drug),
        ProductAggregate.Register(MarketingAuthorizationHolder, "OZE-1", "Ozempic", ProductType.Drug),
    ];
}
