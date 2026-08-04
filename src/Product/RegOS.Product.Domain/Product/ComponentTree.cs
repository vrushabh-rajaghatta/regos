using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// The components of one market, as the shape they form.
/// </summary>
/// <remarks>
/// <b>The rules of a hierarchy are statements about a tree, not about a
/// node</b> — <em>"nothing may be its own ancestor"</em> and <em>"a tree may be
/// three levels deep"</em> cannot be checked by a component that can only see
/// itself. Each <see cref="MedicinalProductComponent"/> is its own root
/// (EPIC-010a D5), so this is the domain type those statements are about, and
/// it is passed into the operations that would change the shape.
/// <para>
/// <b>Nothing here is persisted.</b> It is built from a list the application
/// already loaded, so the acyclicity and the depth limit live in behaviour
/// rather than in a constraint the database would have to express — which
/// Postgres cannot do for a recursive adjacency list without a trigger.
/// </para>
/// <para>
/// <b>One home for traversal, deliberately.</b> There are exactly two walks
/// here — upward to the ancestors, and one level down to the children — and
/// every rule and read is expressed in terms of them. Several recursive
/// helpers scattered across handlers would each be correct and collectively
/// unreviewable.
/// </para>
/// </remarks>
public sealed class ComponentTree
{
    /// <summary>
    /// How deep a component tree may be, counting a top-level article as one.
    /// </summary>
    /// <remarks>
    /// <b>A domain rule, not a database limitation.</b> The schema represents
    /// whatever tree it is given; this decides which trees RegOS will accept,
    /// so changing it is a decision rather than a migration.
    /// <para>
    /// Three, because that is one level past anything demonstrated. A
    /// pre-filled pen is one; a kit holding a vial and a solvent is two; three
    /// leaves room for a co-packaged assembly nobody has shown us yet. Beyond
    /// that the model is more likely wrong than the limit.
    /// </para>
    /// </remarks>
    public const int MaxDepth = 3;

    private readonly Dictionary<MedicinalProductComponentId, MedicinalProductComponent> _byId;

    private ComponentTree(
        Dictionary<MedicinalProductComponentId, MedicinalProductComponent> byId)
    {
        _byId = byId;
    }

    /// <summary>
    /// Builds the tree from every component of one market.
    /// </summary>
    /// <remarks>
    /// It must be <em>every</em> component: a partial list would make a cycle
    /// undetectable and a depth check optimistic. That is why the repository
    /// loads by market rather than by id.
    /// </remarks>
    public static ComponentTree Of(IEnumerable<MedicinalProductComponent> components)
        => new(components.ToDictionary(x => x.Id));

    /// <summary>
    /// The chain from <paramref name="id"/> upward, nearest parent first.
    /// </summary>
    /// <remarks>
    /// Guarded against a cycle it did not create. The rules below make one
    /// impossible, but this walk runs over data loaded from a database that has
    /// no such guarantee — and a hang is a far worse failure than a refusal.
    /// </remarks>
    public IReadOnlyList<MedicinalProductComponentId> AncestorsOf(
        MedicinalProductComponentId id)
    {
        var ancestors = new List<MedicinalProductComponentId>();
        var seen = new HashSet<MedicinalProductComponentId> { id };

        var current = _byId.TryGetValue(id, out var component)
            ? component.ParentComponentId
            : null;

        while (current is not null && seen.Add(current))
        {
            ancestors.Add(current);

            current = _byId.TryGetValue(current, out var parent)
                ? parent.ParentComponentId
                : null;
        }

        return ancestors;
    }

    public IReadOnlyList<MedicinalProductComponent> ChildrenOf(
        MedicinalProductComponentId id)
        => _byId.Values.Where(x => x.ParentComponentId == id).ToList();

    /// <summary>
    /// Every component, parents before their contents, alphabetical among
    /// siblings — the order a person reads a packing list in, each paired with
    /// its depth.
    /// </summary>
    /// <remarks>
    /// <b>Here rather than in the query handler, and that placement is the
    /// point.</b> A depth-first walk in a handler would have been the third
    /// traversal of this tree written in a third place, each correct on its own
    /// and collectively unreviewable. The read and the rules now walk the same
    /// structure, so a row's depth on screen and the depth the guard measured
    /// cannot drift apart.
    /// <para>
    /// Sorting by name alone would scatter a kit's contents through the list;
    /// the nesting is what makes a packing list readable.
    /// </para>
    /// </remarks>
    public IReadOnlyList<(MedicinalProductComponent Component, int Depth)>
        InReadingOrder()
    {
        var ordered = new List<(MedicinalProductComponent, int)>();
        var seen = new HashSet<MedicinalProductComponentId>();

        void Append(MedicinalProductComponentId? parentId, int depth)
        {
            var children = parentId is null
                ? _byId.Values.Where(x => x.ParentComponentId is null)
                : ChildrenOf(parentId);

            foreach (var component in children.OrderBy(
                x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                // Cycle-safe for the same reason as every other walk here: the
                // rules make one impossible, the database does not.
                if (!seen.Add(component.Id))
                    continue;

                ordered.Add((component, depth));

                Append(component.Id, depth + 1);
            }
        }

        Append(null, 1);

        return ordered;
    }

    /// <summary>
    /// How deep <paramref name="parentId"/> sits — zero when nothing is named,
    /// meaning a component placed there would be top-level.
    /// </summary>
    public int DepthUnder(MedicinalProductComponentId? parentId)
        => parentId is null ? 0 : AncestorsOf(parentId).Count + 1;

    /// <summary>
    /// Refuses to add a component beneath <paramref name="parentId"/> when
    /// there is no room, or when the parent is not part of this market.
    /// </summary>
    public void RequireRoomBeneath(MedicinalProductComponentId? parentId)
    {
        if (parentId is not null && !_byId.ContainsKey(parentId))
            throw new NotFoundException(
                MedicinalProductComponentErrors.ParentNotFound);

        if (DepthUnder(parentId) + 1 > MaxDepth)
            throw new BusinessRuleViolationException(
                MedicinalProductComponentErrors.TooDeep);
    }

    /// <summary>
    /// Refuses a move that would make a component its own ancestor, or push
    /// part of the tree past <see cref="MaxDepth"/>.
    /// </summary>
    /// <remarks>
    /// <b>The subtree moves with it</b>, so the depth check measures the moved
    /// component's own height rather than treating it as a leaf. Moving a kit
    /// two levels down takes its contents with it, and it is the contents that
    /// would fall off the end.
    /// </remarks>
    public void RequireCanReparent(
        MedicinalProductComponentId componentId,
        MedicinalProductComponentId? newParentId)
    {
        if (newParentId is null)
            return;

        if (!_byId.ContainsKey(newParentId))
            throw new NotFoundException(
                MedicinalProductComponentErrors.ParentNotFound);

        // Its own id counts: a component may not be placed inside itself, and
        // the ancestors of the target say whether it is inside this one.
        if (newParentId == componentId
            || AncestorsOf(newParentId).Contains(componentId))
        {
            throw new BusinessRuleViolationException(
                MedicinalProductComponentErrors.WouldBeItsOwnAncestor);
        }

        if (DepthUnder(newParentId) + HeightOf(componentId, []) > MaxDepth)
            throw new BusinessRuleViolationException(
                MedicinalProductComponentErrors.TooDeep);
    }

    /// <summary>
    /// Refuses to remove a component that still holds others.
    /// </summary>
    public void RequireNothingInside(MedicinalProductComponentId componentId)
    {
        if (ChildrenOf(componentId).Count > 0)
            throw new BusinessRuleViolationException(
                MedicinalProductComponentErrors.StillHoldsComponents);
    }

    /// <summary>
    /// How many levels this component spans, itself included — one for a
    /// component holding nothing.
    /// </summary>
    /// <remarks>
    /// Carries a visited set for the same reason <see cref="AncestorsOf"/>
    /// does: the rules above make a cycle impossible, but this walks data
    /// loaded from a database that carries no such guarantee, and unbounded
    /// recursion fails far worse than a refusal.
    /// </remarks>
    private int HeightOf(
        MedicinalProductComponentId id,
        HashSet<MedicinalProductComponentId> seen)
    {
        if (!seen.Add(id))
            return 0;

        var children = ChildrenOf(id);

        return children.Count == 0
            ? 1
            : 1 + children.Max(child => HeightOf(child.Id, seen));
    }
}
