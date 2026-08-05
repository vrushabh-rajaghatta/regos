using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// The layers of one pack, as the shape they form.
/// </summary>
/// <remarks>
/// <b>The second recursive structure in RegOS, and deliberately not an
/// abstraction over the first</b>
/// (<see href="../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>
/// §2). <see cref="ComponentTree"/>'s <em>pattern</em> is copied — a
/// non-persisted domain type built from a full load, carrying the depth guard,
/// the cycle guard and the reading order, because the rules of a hierarchy are
/// statements about a tree rather than about a node.
/// <para>
/// <b>Its code is not copied, and the divergence is already visible.</b> This
/// tree allows four layers where a component tree allows three, and it orders
/// siblings by what holds most rather than alphabetically. A generic
/// <c>RecursiveTree&lt;T&gt;</c> written now would be an abstraction over one
/// demonstrated case and one that has already drifted from it —
/// <see href="../../../docs/adr/ADR-018-rule-of-three.md">ADR-018</see> says
/// duplicate on the second and evaluate on the third, and this is the second.
/// </para>
/// <para>
/// <b>Nothing here is persisted.</b> Acyclicity lives in behaviour rather than
/// in a constraint, which Postgres cannot express for an adjacency list without
/// a trigger.
/// </para>
/// </remarks>
public sealed class PackagingTree
{
    /// <summary>
    /// How deep a pack may be, counting the outermost layer as one.
    /// </summary>
    /// <remarks>
    /// <b>Four, and one more than a component tree — the first place the two
    /// structures visibly differ.</b> A carton holding blisters is two; a carton
    /// holding wallets holding blisters is three; four leaves room for a shipper
    /// above a carton, which a supply chain has and nobody has yet asked RegOS
    /// to record. Beyond that the model is more likely wrong than the limit.
    /// <para>
    /// A domain rule, not a schema limitation: changing it is a decision rather
    /// than a migration.
    /// </para>
    /// </remarks>
    public const int MaxDepth = 4;

    private readonly Dictionary<PackageItemId, PackageItem> _byId;

    private PackagingTree(Dictionary<PackageItemId, PackageItem> byId)
    {
        _byId = byId;
    }

    /// <summary>
    /// Builds the tree from every layer of one pack.
    /// </summary>
    /// <remarks>
    /// It must be <em>every</em> layer: a partial list would make a cycle
    /// undetectable and a depth check optimistic. That is why the repository
    /// loads by pack rather than by id.
    /// </remarks>
    public static PackagingTree Of(IEnumerable<PackageItem> items)
        => new(items.ToDictionary(x => x.Id));

    /// <summary>
    /// The chain from <paramref name="id"/> outward, nearest parent first.
    /// </summary>
    /// <remarks>
    /// Guarded against a cycle it did not create. The rules below make one
    /// impossible, but this walks data loaded from a database that carries no
    /// such guarantee, and a hang is a far worse failure than a refusal.
    /// </remarks>
    public IReadOnlyList<PackageItemId> AncestorsOf(PackageItemId id)
    {
        var ancestors = new List<PackageItemId>();
        var seen = new HashSet<PackageItemId> { id };

        var current = _byId.TryGetValue(id, out var item)
            ? item.ParentPackageItemId
            : null;

        while (current is not null && seen.Add(current))
        {
            ancestors.Add(current);

            current = _byId.TryGetValue(current, out var parent)
                ? parent.ParentPackageItemId
                : null;
        }

        return ancestors;
    }

    public IReadOnlyList<PackageItem> ChildrenOf(PackageItemId id)
        => _byId.Values.Where(x => x.ParentPackageItemId == id).ToList();

    /// <summary>
    /// Every layer, outermost first, each paired with its depth — the order a
    /// person opens a box in.
    /// </summary>
    /// <remarks>
    /// <b>Siblings are ordered by quantity, most first</b>, and that is the
    /// second place this tree differs from a component tree. A packing list
    /// reads <em>"3 blisters, 1 leaflet"</em>; alphabetical order would put the
    /// leaflet before the medicine.
    /// </remarks>
    public IReadOnlyList<(PackageItem Item, int Depth)> InReadingOrder()
    {
        var ordered = new List<(PackageItem, int)>();
        var seen = new HashSet<PackageItemId>();

        void Append(PackageItemId? parentId, int depth)
        {
            var children = parentId is null
                ? _byId.Values.Where(x => x.ParentPackageItemId is null)
                : ChildrenOf(parentId);

            foreach (var item in children
                .OrderByDescending(x => x.Quantity)
                .ThenBy(x => x.ItemType.Display, StringComparer.OrdinalIgnoreCase))
            {
                // Cycle-safe for the same reason as every other walk here.
                if (!seen.Add(item.Id))
                    continue;

                ordered.Add((item, depth));

                Append(item.Id, depth + 1);
            }
        }

        Append(null, 1);

        return ordered;
    }

    /// <summary>
    /// How deep <paramref name="parentId"/> sits — zero when nothing is named,
    /// meaning a layer placed there would be the outermost.
    /// </summary>
    public int DepthUnder(PackageItemId? parentId)
        => parentId is null ? 0 : AncestorsOf(parentId).Count + 1;

    /// <summary>
    /// Refuses to add a layer beneath <paramref name="parentId"/> when there is
    /// no room, or when that layer is not part of this pack.
    /// </summary>
    public void RequireRoomBeneath(PackageItemId? parentId)
    {
        if (parentId is not null && !_byId.ContainsKey(parentId))
            throw new NotFoundException(PackageItemErrors.ParentNotFound);

        if (DepthUnder(parentId) + 1 > MaxDepth)
            throw new BusinessRuleViolationException(PackageItemErrors.TooDeep);
    }

    /// <summary>
    /// Refuses a move that would put a layer inside itself, or push part of the
    /// pack past <see cref="MaxDepth"/>.
    /// </summary>
    public void RequireCanReparent(
        PackageItemId itemId,
        PackageItemId? newParentId)
    {
        if (newParentId is null)
            return;

        if (!_byId.ContainsKey(newParentId))
            throw new NotFoundException(PackageItemErrors.ParentNotFound);

        // Its own id counts: a layer may not be placed inside itself, and the
        // ancestors of the target say whether it is already inside this one.
        if (newParentId == itemId
            || AncestorsOf(newParentId).Contains(itemId))
        {
            throw new BusinessRuleViolationException(
                PackageItemErrors.WouldBeItsOwnAncestor);
        }

        if (DepthUnder(newParentId) + HeightOf(itemId, []) > MaxDepth)
            throw new BusinessRuleViolationException(PackageItemErrors.TooDeep);
    }

    /// <summary>
    /// Refuses to remove a layer that still holds others.
    /// </summary>
    public void RequireNothingInside(PackageItemId itemId)
    {
        if (ChildrenOf(itemId).Count > 0)
            throw new BusinessRuleViolationException(
                PackageItemErrors.StillHoldsItems);
    }

    /// <summary>
    /// How many layers this one spans, itself included — one for a layer
    /// holding nothing.
    /// </summary>
    private int HeightOf(PackageItemId id, HashSet<PackageItemId> seen)
    {
        if (!seen.Add(id))
            return 0;

        var children = ChildrenOf(id);

        return children.Count == 0
            ? 1
            : 1 + children.Max(child => HeightOf(child.Id, seen));
    }
}
