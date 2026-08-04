using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// The rules of the hierarchy, which live on the tree because that is what they
/// are statements about — a component that can only see itself cannot know
/// whether it is its own ancestor.
/// </summary>
public class ComponentTreeTests
{
    private static readonly TenantId Tenant = TenantId.From(Guid.NewGuid());
    private static readonly MedicinalProductId Market = MedicinalProductId.New();

    private static CodedConcept Kit() => CodedConcept.Internal("KIT", "Kit");

    /// <summary>
    /// Builds a component through the aggregate, which is the only way one can
    /// be made — so every fixture here is also exercising the create guard.
    /// </summary>
    private static MedicinalProductComponent Component(
        ComponentTree tree,
        string name,
        MedicinalProductComponentId? parentId = null)
        => MedicinalProductComponent.Create(
            Tenant, Market, parentId, Kit(), name, null, 1m, null, null, tree);

    private static ComponentTree TreeOf(params MedicinalProductComponent[] parts)
        => ComponentTree.Of(parts);

    [Fact]
    public void ATopLevelComponentIsWhatThePatientIsHanded()
    {
        var component = Component(TreeOf(), "Combination pack");

        component.IsTopLevel.Should().BeTrue();
        component.ParentComponentId.Should().BeNull();
    }

    [Fact]
    public void AComponentCanBePlacedInsideAnother()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var vial = Component(TreeOf(kit), "Vial of powder", kit.Id);

        vial.ParentComponentId.Should().Be(kit.Id);
        TreeOf(kit, vial).ChildrenOf(kit.Id).Should().ContainSingle();
    }

    /// <summary>
    /// The DoD's depth test: a component within a component within a component
    /// is the deepest RegOS accepts, and the fourth is refused.
    /// </summary>
    [Fact]
    public void ThreeLevelsAreAllowedAndAFourthIsRefused()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var tray = Component(TreeOf(kit), "Inner tray", kit.Id);
        var vial = Component(TreeOf(kit, tray), "Vial", tray.Id);

        TreeOf(kit, tray, vial).DepthUnder(vial.Id).Should().Be(3);

        var act = () => Component(TreeOf(kit, tray, vial), "Stopper", vial.Id);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductComponentErrors.TooDeep);
    }

    [Fact]
    public void APlacementInsideSomethingThatDoesNotExistIsRefused()
    {
        var act = () => Component(
            TreeOf(), "Vial", MedicinalProductComponentId.New());

        act.Should().Throw<NotFoundException>()
            .WithMessage(MedicinalProductComponentErrors.ParentNotFound);
    }

    /// <summary>
    /// The cycle every hierarchy has to refuse first.
    /// </summary>
    [Fact]
    public void AComponentCannotBePlacedInsideItself()
    {
        var kit = Component(TreeOf(), "Combination pack");

        var act = () => kit.ReparentTo(kit.Id, TreeOf(kit));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductComponentErrors.WouldBeItsOwnAncestor);
    }

    /// <summary>
    /// The one a depth check alone would miss: neither component gains a level,
    /// so only "am I already inside it?" catches this.
    /// </summary>
    [Fact]
    public void AComponentCannotBePlacedInsideSomethingItContains()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var vial = Component(TreeOf(kit), "Vial", kit.Id);

        var act = () => kit.ReparentTo(vial.Id, TreeOf(kit, vial));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductComponentErrors.WouldBeItsOwnAncestor);
    }

    [Fact]
    public void ADeeperCycleIsAlsoRefused()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var tray = Component(TreeOf(kit), "Inner tray", kit.Id);
        var vial = Component(TreeOf(kit, tray), "Vial", tray.Id);

        var act = () => kit.ReparentTo(vial.Id, TreeOf(kit, tray, vial));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductComponentErrors.WouldBeItsOwnAncestor);
    }

    /// <summary>
    /// A move takes the subtree with it, so the depth check measures the moved
    /// component's height rather than treating it as a leaf. Without that, a
    /// two-level kit could be tucked under another and quietly reach four.
    /// </summary>
    [Fact]
    public void AMoveThatWouldPushItsContentsPastTheLimitIsRefused()
    {
        var outer = Component(TreeOf(), "Outer pack");
        var tray = Component(TreeOf(outer), "Inner tray", outer.Id);

        // A separate two-level assembly: a kit holding a vial.
        var kit = Component(TreeOf(), "Combination pack");
        var vial = Component(TreeOf(kit), "Vial", kit.Id);

        var tree = TreeOf(outer, tray, kit, vial);

        // The kit itself would sit at depth 3 — legal on its own — but the vial
        // inside it would land at 4.
        var act = () => kit.ReparentTo(tray.Id, tree);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductComponentErrors.TooDeep);
    }

    [Fact]
    public void AMoveThatFitsIsAllowed()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var loose = Component(TreeOf(kit), "Syringe");

        loose.ReparentTo(kit.Id, TreeOf(kit, loose));

        loose.ParentComponentId.Should().Be(kit.Id);
    }

    [Fact]
    public void AComponentCanBeMovedBackToTheTopLevel()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var vial = Component(TreeOf(kit), "Vial", kit.Id);

        vial.ReparentTo(null, TreeOf(kit, vial));

        vial.IsTopLevel.Should().BeTrue();
    }

    /// <summary>
    /// Refuses rather than cascading: removing a kit and silently taking its
    /// contents with it is quiet data loss.
    /// </summary>
    [Fact]
    public void AComponentHoldingOthersCannotBeRemoved()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var vial = Component(TreeOf(kit), "Vial", kit.Id);

        var act = () => TreeOf(kit, vial).RequireNothingInside(kit.Id);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductComponentErrors.StillHoldsComponents);
    }

    [Fact]
    public void AnEmptyComponentCanBeRemoved()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var vial = Component(TreeOf(kit), "Vial", kit.Id);

        var act = () => TreeOf(kit, vial).RequireNothingInside(vial.Id);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Parents before their contents, alphabetical among siblings — one walk,
    /// shared by the rules and the read, so a row's depth on screen is the
    /// depth the guard measured.
    /// </summary>
    [Fact]
    public void ReadingOrderNestsContentsUnderTheirHolder()
    {
        var kit = Component(TreeOf(), "Combination pack");
        var vial = Component(TreeOf(kit), "Vial of powder", kit.Id);
        var solvent = Component(TreeOf(kit, vial), "Ampoule of solvent", kit.Id);
        var pen = Component(TreeOf(kit, vial, solvent), "Pre-filled pen");

        var ordered = TreeOf(kit, vial, solvent, pen).InReadingOrder();

        ordered.Select(x => (x.Component.Name, x.Depth))
            .Should().Equal(
                ("Combination pack", 1),
                ("Ampoule of solvent", 2),
                ("Vial of powder", 2),
                ("Pre-filled pen", 1));
    }

    /// <summary>
    /// There is no route to a cycle, which is why the tests above are the whole
    /// of the proof.
    /// </summary>
    /// <remarks>
    /// <b>Written after an attempt to build one failed.</b> A test that forced
    /// two components to point at each other — to exercise the visited-set
    /// guards in the walks — could not be written: every path to that state
    /// goes through <c>RequireCanReparent</c>, and swapping the order only
    /// changes which of the two moves is refused.
    /// <para>
    /// So those guards protect against database state the domain cannot
    /// produce — a hand-written row, a migration defect — and are deliberately
    /// kept unexercised rather than removed. A walk that hangs starves a thread
    /// pool; two lines is a cheap price for a failure mode that noisy.
    /// </para>
    /// </remarks>
    [Fact]
    public void NeitherOrderOfAReciprocalMoveSucceeds()
    {
        var first = Component(TreeOf(), "First");
        var second = Component(TreeOf(first), "Second");

        // Legal on its own — two top-level articles, one moves inside the other.
        first.ReparentTo(second.Id, TreeOf(first, second));

        // And now the move that would close the loop is the one refused.
        var act = () => second.ReparentTo(first.Id, TreeOf(first, second));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(MedicinalProductComponentErrors.WouldBeItsOwnAncestor);
    }
}
