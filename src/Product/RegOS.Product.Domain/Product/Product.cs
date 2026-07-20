using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

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

    /// <summary>
    /// Immutable once registered. The code identifies the product within its
    /// organization - it is the unique key the database enforces - and
    /// organizations cite it in authority correspondence. Changing it is a
    /// distinct business capability with its own rules, not a field edit.
    /// </summary>
    public ProductCode Code { get; }

    public ProductName Name { get; private set; }

    public ProductType Type { get; private set; }

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

    /// <summary>
    /// Reclassifies the product. Separate from <see cref="Rename"/> rather than
    /// a single UpdateDetails: the two carry different intent and will grow
    /// different rules - reclassifying a product that already has applications
    /// or documents is a conversation we have not had yet, and an explicit
    /// method is where that rule will live when we do.
    /// </summary>
    public void ChangeType(ProductType type) => Type = type;

    /// <summary>
    /// Retires the product from new work. It stays readable, and existing
    /// applications, submissions and documents that reference it are
    /// untouched - archiving says "do not start anything new with this", not
    /// "pretend it never existed".
    /// </summary>
    /// <remarks>
    /// Not idempotent, deliberately, and unlike a user's Activate/Deactivate.
    /// Those are toggles: a caller asking for a state the user is already in
    /// has got what it wanted. This is a one-way lifecycle transition, so a
    /// second call is a caller acting on a stale view of the product, and
    /// silently succeeding would hide that. It raises a conflict instead.
    /// </remarks>
    public void Archive()
    {
        if (Status == ProductStatus.Archived)
            throw new BusinessRuleViolationException(
                ProductErrors.AlreadyArchived);

        Status = ProductStatus.Archived;
    }
}
