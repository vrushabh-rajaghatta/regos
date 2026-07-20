using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// A product an organization manages within RegOS. Deliberately small: it
/// answers "what product is this?", not "what regulatory state is it in".
/// Applications, documents, registrations and markets live elsewhere.
/// </summary>
public sealed class Product : AggregateRoot<ProductId>
{
    // Parameterized private constructor, no parameterless one: EF binds by
    // parameter name, and this keeps every field non-nullable without
    // resorting to `= default!`. Same shape as the User aggregate.
    private Product(
        ProductId id,
        OrganizationId organizationId,
        ProductCode code,
        ProductName name,
        ProductType type,
        ProductStatus status)
    {
        Id = id;
        OrganizationId = organizationId;
        Code = code;
        Name = name;
        Type = type;
        Status = status;
    }

    /// <summary>
    /// The owning organization - the tenant. Set once at registration and never
    /// changed: moving a product between organizations would be a transfer, a
    /// different capability with its own rules.
    /// </summary>
    public OrganizationId OrganizationId { get; }

    public ProductCode Code { get; }

    public ProductName Name { get; private set; }

    public ProductType Type { get; }

    public ProductStatus Status { get; private set; }

    public static Product Register(
        OrganizationId organizationId,
        string? code,
        string? name,
        ProductType type)
        => new(
            ProductId.New(),
            organizationId,
            ProductCode.Create(code),
            ProductName.Create(name),
            type,
            ProductStatus.Registered);

    public void Rename(string? name) => Name = ProductName.Create(name);

    /// <summary>Idempotent: archiving an archived product is a no-op.</summary>
    public void Archive()
    {
        if (Status == ProductStatus.Archived)
            return;

        Status = ProductStatus.Archived;
    }
}
