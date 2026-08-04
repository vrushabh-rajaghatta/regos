using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// What the patient physically receives — a vial, a pre-filled pen, the kit
/// that holds a powder and its solvent.
/// </summary>
/// <remarks>
/// <b>The article, not the administrable form.</b>
/// <see cref="PharmaceuticalProductDetail"/> answers <em>"what is this when it
/// is given?"</em>; this answers <em>"what is in the box?"</em> — the same
/// distinction ISO IDMP draws between a pharmaceutical product and a
/// manufactured item. Only that second question justifies the recursion: a kit
/// contains articles, and those articles are themselves things (EPIC-010a Q3).
/// <para>
/// <b>Adjacency list, not a materialised path.</b> Component trees are shallow
/// — a kit holding two articles — and a path column is a write-time cost paid
/// for a read that a two-level tree does not need (D5). A recursive CTE is
/// where this goes if depth ever exceeds what one query handles, and not
/// before.
/// </para>
/// <para>
/// <b>The rules about shape live on <see cref="ComponentTree"/>, and are passed
/// in.</b> <em>"Nothing may be its own ancestor"</em> and <em>"a tree may be
/// three levels deep"</em> are statements about a tree; a component that can
/// only see itself cannot check either. Every operation that changes the shape
/// takes the tree, so the guard and the mutation cannot be separated — which is
/// what keeps acyclicity a behaviour rather than something the schema would
/// have to express.
/// </para>
/// <para>
/// <b>No ingredients here.</b> RIM allows a composition beneath a component;
/// nothing demonstrates it, and Q3 asks what the patient receives rather than
/// what a component is made of. One demonstrated parent is not two
/// (EPIC-010a D3).
/// </para>
/// </remarks>
public sealed class MedicinalProductComponent
    : AggregateRoot<MedicinalProductComponentId>
{
    public const int NameMaxLength = 250;
    public const int DescriptionMaxLength = 2000;

    private MedicinalProductComponent()
    {
    }

    /// <summary>The owning tenant (ADR-031). Fail-closed, set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>
    /// The market this article belongs to. Immutable — and the reason a
    /// component may only be placed inside another from the same market.
    /// </summary>
    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    /// <summary>
    /// What holds this one, or null when it is what the patient is handed.
    /// </summary>
    public MedicinalProductComponentId? ParentComponentId { get; private set; }

    /// <summary>The kind of article. Screen word: <b>Type</b>.</summary>
    public CodedConcept ComponentType { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    /// <summary>How many of it. One vial, two ampoules.</summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// What the quantity counts, when the type does not already say it.
    /// </summary>
    public CodedConcept? UnitOfPresentation { get; private set; }

    /// <summary>
    /// The form of what is inside — a vial of powder, an ampoule of solution.
    /// Optional, and different from the presentation's: a kit's halves have
    /// their own forms, and the reconstituted product has a third.
    /// </summary>
    public CodedConcept? DoseForm { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// True when this is what the patient is handed rather than something
    /// inside it.
    /// </summary>
    public bool IsTopLevel => ParentComponentId is null;

    /// <param name="tree">
    /// Every component this market already has. Required even to create a
    /// top-level article, because it is what says whether there is room
    /// beneath the named parent — and a partial tree would make that answer
    /// optimistic.
    /// </param>
    public static MedicinalProductComponent Create(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        MedicinalProductComponentId? parentComponentId,
        CodedConcept componentType,
        string name,
        string? description,
        decimal quantity,
        CodedConcept? unitOfPresentation,
        CodedConcept? doseForm,
        ComponentTree tree)
    {
        if (tenantId is null)
            throw new DomainException(
                MedicinalProductComponentErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(
                MedicinalProductComponentErrors.MarketRequired);

        tree.RequireRoomBeneath(parentComponentId);

        var component = new MedicinalProductComponent
        {
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            ParentComponentId = parentComponentId,
            CreatedOn = DateTime.UtcNow
        };

        component.Restate(
            componentType, name, description, quantity, unitOfPresentation, doseForm);

        component.Id = MedicinalProductComponentId.New();

        return component;
    }

    /// <summary>
    /// Restates everything about the article except where it sits.
    /// </summary>
    /// <remarks>
    /// Position is deliberately not here. Moving a component is the operation
    /// with a rule attached, and folding it into a general update would let a
    /// caller change the tree's shape without passing the tree.
    /// </remarks>
    public void Restate(
        CodedConcept componentType,
        string name,
        string? description,
        decimal quantity,
        CodedConcept? unitOfPresentation,
        CodedConcept? doseForm)
    {
        if (componentType is null)
            throw new DomainException(
                MedicinalProductComponentErrors.ComponentTypeRequired);

        if (quantity <= 0)
            throw new DomainException(
                MedicinalProductComponentErrors.QuantityMustBePositive);

        ComponentType = componentType;
        Name = ValidatedName(name);
        Description = ValidatedDescription(description);
        Quantity = quantity;
        UnitOfPresentation = unitOfPresentation;
        DoseForm = doseForm;
    }

    /// <summary>
    /// Moves this article — and everything inside it — somewhere else in the
    /// same market, or to the top level.
    /// </summary>
    /// <remarks>
    /// <b>The tree refuses, not this method</b>, and that is the point: the
    /// question <em>"would this create a cycle?"</em> has no answer from here.
    /// Taking the tree as a parameter is what makes it impossible to move a
    /// component without asking.
    /// </remarks>
    public void ReparentTo(
        MedicinalProductComponentId? newParentComponentId, ComponentTree tree)
    {
        tree.RequireCanReparent(Id, newParentComponentId);

        ParentComponentId = newParentComponentId;
    }

    private static string ValidatedName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(
                MedicinalProductComponentErrors.NameRequired);

        var trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
            throw new DomainException(
                MedicinalProductComponentErrors.NameTooLong);

        return trimmed;
    }

    private static string? ValidatedDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();

        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException(
                MedicinalProductComponentErrors.DescriptionTooLong);

        return trimmed;
    }
}
