using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// One layer of a pack — the carton, the blisters inside it, the wallet
/// holding those.
/// </summary>
/// <remarks>
/// <b>Not a component, and the difference is the whole reason this type
/// exists</b>
/// (<see href="../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>
/// §1). <em>Does it change when the same medicine is sold in a different pack
/// size?</em> A 30-tablet carton and a 100-tablet carton share an identical
/// <see cref="MedicinalProductComponent"/> tree and differ entirely here.
/// <para>
/// Stated as a pair: <b>a component has a dose form; a package item has a
/// material.</b> That is why <see cref="Material"/> is on this type and nowhere
/// near a component.
/// </para>
/// <para>
/// <b>No <c>Name</c>, unlike a component.</b> A component is a named article —
/// <em>"Solvent vial"</em>. A layer of a pack is a thing of a known kind, and
/// <see cref="ItemType"/> is that kind: <em>Blister</em> names it exactly.
/// <see cref="Description"/> is there for what the code cannot say —
/// <em>"child-resistant closure"</em>.
/// </para>
/// <para>
/// <b>The rules about shape live on <see cref="PackagingTree"/> and are passed
/// in</b>, exactly as <see cref="ComponentTree"/> does one aggregate over.
/// <em>"Nothing may be its own ancestor"</em> is a statement about a tree, and
/// an item that can only see itself cannot check it. **The pattern is copied,
/// the code is not** — the two trees guard different depths and would diverge
/// further under one abstraction (ADR-061 §2, ADR-018).
/// </para>
/// </remarks>
public sealed class PackageItem : AggregateRoot<PackageItemId>
{
    public const int DescriptionMaxLength = 500;

    // Parameterless with an object-initializer factory: an owned value object
    // cannot bind to a constructor parameter, and ItemType is one.
    private PackageItem()
    {
    }

    /// <summary>The owning tenant (ADR-031). Fail-closed, set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>The pack this is a layer of. Immutable.</summary>
    public PackagedProductId PackagedProductId { get; private set; } = default!;

    /// <summary>
    /// The layer that holds this one. Null for the outermost — what a
    /// dispenser takes off the shelf.
    /// </summary>
    public PackageItemId? ParentPackageItemId { get; private set; }

    /// <summary>What this layer is: a carton, a blister, a bottle.</summary>
    public CodedConcept ItemType { get; private set; } = default!;

    /// <summary>
    /// What it is made of. <b>The attribute that makes this not a component.</b>
    /// Null is ordinary — an outer carton's board grade is rarely stated, while
    /// a blister's laminate always is, because it is what the stability data
    /// was generated against.
    /// </summary>
    public CodedConcept? Material { get; private set; }

    /// <summary>
    /// How many of these sit inside the layer above — three blisters in the
    /// carton. At least one; a layer nobody has any of is not a layer.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// What the quantity counts, when it is not simply "of these". Null is
    /// ordinary: three blisters need no unit.
    /// </summary>
    public CodedConcept? UnitOfPresentation { get; private set; }

    /// <summary>
    /// What the codes cannot say — <em>"child-resistant closure"</em>,
    /// <em>"with integrated desiccant"</em>.
    /// </summary>
    public string? Description { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public static PackageItem Create(
        TenantId tenantId,
        PackagedProductId packagedProductId,
        PackageItemId? parentPackageItemId,
        CodedConcept itemType,
        CodedConcept? material,
        decimal quantity,
        CodedConcept? unitOfPresentation,
        string? description,
        PackagingTree tree)
    {
        if (tenantId is null)
            throw new DomainException(PackageItemErrors.TenantRequired);

        if (packagedProductId is null)
            throw new DomainException(PackageItemErrors.PackRequired);

        // The guard and the mutation cannot be separated: the tree is built
        // from every layer of this pack, so "is there room" and "is that parent
        // ours" are both answered rather than assumed.
        tree.RequireRoomBeneath(parentPackageItemId);

        var item = new PackageItem
        {
            Id = PackageItemId.New(),
            TenantId = tenantId,
            PackagedProductId = packagedProductId,
            ParentPackageItemId = parentPackageItemId,
            CreatedOnUtc = DateTime.UtcNow
        };

        item.Restate(itemType, material, quantity, unitOfPresentation, description);

        return item;
    }

    /// <summary>
    /// Restates everything about the layer except where it sits.
    /// </summary>
    /// <remarks>
    /// Moving is <see cref="MoveTo"/>, because where a layer sits is a
    /// statement about the tree and needs the tree to check it.
    /// </remarks>
    public void Restate(
        CodedConcept itemType,
        CodedConcept? material,
        decimal quantity,
        CodedConcept? unitOfPresentation,
        string? description)
    {
        if (itemType is null)
            throw new DomainException(
                PackagingVocabularyErrors.UnknownPackageItemType(null));

        if (quantity <= 0)
            throw new DomainException(PackageItemErrors.QuantityMustBePositive);

        if (description is not null
            && description.Trim().Length > DescriptionMaxLength)
        {
            throw new DomainException(PackageItemErrors.DescriptionTooLong);
        }

        ItemType = itemType;
        Material = material;
        Quantity = quantity;
        UnitOfPresentation = unitOfPresentation;
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    /// <summary>
    /// Moves this layer, and everything inside it, under another.
    /// </summary>
    /// <remarks>
    /// The subtree travels with it, which is why
    /// <see cref="PackagingTree.RequireCanReparent"/> measures this layer's own
    /// height rather than treating it as a leaf.
    /// </remarks>
    public void MoveTo(PackageItemId? newParentId, PackagingTree tree)
    {
        tree.RequireCanReparent(Id, newParentId);

        ParentPackageItemId = newParentId;
    }
}
