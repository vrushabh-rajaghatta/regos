using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// The second recursive structure in RegOS, and the tests that show it is a
/// second <em>structure</em> rather than a second copy.
/// </summary>
/// <remarks>
/// The rules are `ComponentTree`'s pattern; the numbers are not. This tree
/// allows four layers where a component tree allows three, and orders siblings
/// by quantity rather than by name — the divergence ADR-061 §2 cites as the
/// reason not to abstract on the second occurrence.
/// </remarks>
public sealed class PackagingTreeTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly PackagedProductId Pack = PackagedProductId.New();

    private static readonly CodedConcept Carton =
        PackagingVocabulary.PackageItemTypeOf("CARTON")!;

    private static readonly CodedConcept Blister =
        PackagingVocabulary.PackageItemTypeOf("BLISTER")!;

    private static readonly CodedConcept Wallet =
        PackagingVocabulary.PackageItemTypeOf("WALLET")!;

    private static readonly CodedConcept Shipper =
        PackagingVocabulary.PackageItemTypeOf("SHIPPER")!;

    private static PackageItem Layer(
        CodedConcept type,
        PackageItemId? parent,
        IEnumerable<PackageItem> existing,
        decimal quantity = 1,
        CodedConcept? material = null)
        => PackageItem.Create(
            Tenant, Pack, parent, type, material, quantity, null, null,
            PackagingTree.Of(existing));

    // --- what a layer is -----------------------------------------------------

    /// <summary>
    /// <b>The attribute that makes this not a component</b> (ADR-061 §1): a
    /// component has a dose form, a package item has a material.
    /// </summary>
    [Fact]
    public void ALayerCarriesAMaterialWhereAComponentCarriesADoseForm()
    {
        var blister = Layer(
            Blister, null, [], 3,
            PackagingVocabulary.MaterialOf("PVC_ALU"));

        blister.Material!.Display.Should().Be("PVC/aluminium");

        typeof(PackageItem).GetProperties()
            .Select(x => x.Name)
            .Should().NotContain("DoseForm");
    }

    /// <summary>
    /// Null is ordinary: an outer carton's board grade is rarely stated, while a
    /// blister's laminate always is.
    /// </summary>
    [Fact]
    public void AMaterialIsOptional()
    {
        Layer(Carton, null, []).Material.Should().BeNull();
    }

    [Fact]
    public void ALayerNobodyHasAnyOfIsRefused()
    {
        var create = () => Layer(Carton, null, [], quantity: 0);

        create.Should().Throw<DomainException>()
            .WithMessage(PackageItemErrors.QuantityMustBePositive);
    }

    // --- depth ---------------------------------------------------------------

    /// <summary>
    /// <b>Four, one more than a component tree — the first place the two
    /// structures visibly differ.</b> Shipper → carton → wallet → blister is
    /// exactly four and is allowed; a fifth is not.
    /// </summary>
    [Fact]
    public void FourLayersAreAllowedAndAFifthIsNot()
    {
        var shipper = Layer(Shipper, null, []);
        var carton = Layer(Carton, shipper.Id, [shipper]);
        var wallet = Layer(Wallet, carton.Id, [shipper, carton]);
        var blister = Layer(Blister, wallet.Id, [shipper, carton, wallet]);

        var fifth = () => Layer(
            Blister, blister.Id, [shipper, carton, wallet, blister]);

        fifth.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PackageItemErrors.TooDeep);
    }

    /// <summary>
    /// The limit differs from <c>ComponentTree.MaxDepth</c>, and that is the
    /// evidence for duplicating rather than abstracting: one shared constant
    /// would already be wrong for one of them.
    /// </summary>
    [Fact]
    public void TheTwoTreesDoNotShareADepthLimit()
    {
        PackagingTree.MaxDepth.Should().NotBe(ComponentTree.MaxDepth);
    }

    [Fact]
    public void AParentFromAnotherPackIsRefused()
    {
        var stranger = Layer(Carton, null, []);

        var create = () => Layer(Blister, stranger.Id, []);

        create.Should().Throw<NotFoundException>()
            .WithMessage(PackageItemErrors.ParentNotFound);
    }

    // --- moving --------------------------------------------------------------

    [Fact]
    public void ALayerCannotBePlacedInsideItself()
    {
        var carton = Layer(Carton, null, []);

        var move = () => carton.MoveTo(carton.Id, PackagingTree.Of([carton]));

        move.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PackageItemErrors.WouldBeItsOwnAncestor);
    }

    [Fact]
    public void ALayerCannotBePlacedInsideItsOwnContents()
    {
        var carton = Layer(Carton, null, []);
        var blister = Layer(Blister, carton.Id, [carton]);

        var move = () => carton.MoveTo(
            blister.Id, PackagingTree.Of([carton, blister]));

        move.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PackageItemErrors.WouldBeItsOwnAncestor);
    }

    /// <summary>
    /// <b>The subtree travels with it</b>, so the depth check measures the moved
    /// layer's own height rather than treating it as a leaf.
    /// </summary>
    [Fact]
    public void AMoveThatWouldPushTheContentsPastTheLimitIsRefused()
    {
        // A carton two layers tall: carton -> wallet -> blister.
        var carton = Layer(Carton, null, []);
        var wallet = Layer(Wallet, carton.Id, [carton]);
        var blister = Layer(Blister, wallet.Id, [carton, wallet]);

        // A shipper holding another shipper is already two deep.
        var outer = Layer(Shipper, null, [carton, wallet, blister]);
        var inner = Layer(
            Shipper, outer.Id, [carton, wallet, blister, outer]);

        var all = new[] { carton, wallet, blister, outer, inner };

        // Moving the three-tall carton under a two-deep layer needs five.
        var move = () => carton.MoveTo(inner.Id, PackagingTree.Of(all));

        move.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PackageItemErrors.TooDeep);
    }

    [Fact]
    public void ALayerCanBeLiftedToTheOutermostLevel()
    {
        var carton = Layer(Carton, null, []);
        var blister = Layer(Blister, carton.Id, [carton]);

        blister.MoveTo(null, PackagingTree.Of([carton, blister]));

        blister.ParentPackageItemId.Should().BeNull();
    }

    // --- removing ------------------------------------------------------------

    /// <summary>
    /// Refused rather than cascaded: removing a carton that still holds blisters
    /// would silently take them with it.
    /// </summary>
    [Fact]
    public void ALayerThatStillHoldsOthersCannotBeRemoved()
    {
        var carton = Layer(Carton, null, []);
        var blister = Layer(Blister, carton.Id, [carton]);

        var remove = () => PackagingTree.Of([carton, blister])
            .RequireNothingInside(carton.Id);

        remove.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PackageItemErrors.StillHoldsItems);
    }

    // --- reading order -------------------------------------------------------

    /// <summary>
    /// <b>Siblings are ordered by quantity, most first</b> — the second place
    /// this tree differs from a component tree, which sorts alphabetically. A
    /// packing list reads <em>"3 blisters, 1 leaflet"</em>; by name the leaflet
    /// would come before the medicine.
    /// </summary>
    [Fact]
    public void SiblingsReadMostFirstRatherThanAlphabetically()
    {
        var carton = Layer(Carton, null, []);
        var leaflet = Layer(Wallet, carton.Id, [carton], quantity: 1);
        var blisters = Layer(
            Blister, carton.Id, [carton, leaflet], quantity: 3);

        var order = PackagingTree.Of([carton, leaflet, blisters])
            .InReadingOrder()
            .Select(row => row.Item.Id)
            .ToList();

        order.Should().Equal(carton.Id, blisters.Id, leaflet.Id);
    }

    /// <summary>
    /// Depth is computed by the same tree the rules use, so a row's indentation
    /// on screen and the depth the guard measured cannot drift apart.
    /// </summary>
    [Fact]
    public void ReadingOrderCarriesTheDepthTheGuardMeasured()
    {
        var carton = Layer(Carton, null, []);
        var blister = Layer(Blister, carton.Id, [carton]);

        var rows = PackagingTree.Of([carton, blister]).InReadingOrder();

        rows.Should().HaveCount(2);
        rows[0].Depth.Should().Be(1);
        rows[1].Depth.Should().Be(2);
    }

    /// <summary>
    /// The walks carry a visited set because they run over data loaded from a
    /// database that guarantees no acyclicity — a hang is a far worse failure
    /// than a refusal.
    /// </summary>
    [Fact]
    public void AWalkOverACycleTerminates()
    {
        var first = Layer(Carton, null, []);
        var second = Layer(Blister, first.Id, [first]);

        // Forced past the guards, the way a hand-edited row would be.
        typeof(PackageItem)
            .GetProperty(nameof(PackageItem.ParentPackageItemId))!
            .SetValue(first, second.Id);

        var walk = () => PackagingTree.Of([first, second])
            .AncestorsOf(first.Id);

        walk.Should().NotThrow();
    }
}
