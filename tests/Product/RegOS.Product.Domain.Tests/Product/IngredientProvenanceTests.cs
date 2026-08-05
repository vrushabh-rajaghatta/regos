using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Tests.Product;

/// <summary>
/// EPIC-010c S003 — where an ingredient's substance comes from.
/// </summary>
/// <remarks>
/// <b>D2, asserted rather than argued.</b> An ingredient's source and the sites
/// that perform operations for the finished product are <em>different stages of
/// the supply chain</em>
/// (<see href="../../../../docs/adr/ADR-063-where-a-product-is-made-is-a-product-fact.md">ADR-063</see>
/// §2), and the case that proves it is ordinary rather than exotic:
/// <code>
/// Finished product          made at Site Gamma
/// ├── API A                 from Site Alpha
/// └── API B                 from Site Beta
/// </code>
/// <b>This also takes a seam the type recorded in EPIC-010a</b>, which named its
/// own trigger — <em>"sourcing belongs to cluster D"</em>. Cluster D is this
/// epic, and it asked.
/// </remarks>
public sealed class IngredientProvenanceTests
{
    private static readonly OrganizationSiteId Alpha = OrganizationSiteId.New();
    private static readonly OrganizationSiteId Beta = OrganizationSiteId.New();

    private static Strength Mg(decimal value)
        => Strength.Create(value, CodedConcept.Internal("MG", "mg"));

    private static PharmaceuticalProductDetail Presentation()
        => PharmaceuticalProductDetail.Create(
            TenantId.From(Guid.NewGuid()),
            MedicinalProductId.New(),
            "Film-coated tablet",
            null,
            CodedConcept.Internal("TABLET", "Tablet"),
            null,
            []);

    /// <summary>
    /// <b>The case D2 exists for.</b> Two actives from two sites, in one
    /// composition — which no set of finished-product operations could express,
    /// because an operation names the product and not the substance.
    /// </summary>
    [Fact]
    public void TwoActivesMayComeFromTwoDifferentSites()
    {
        var presentation = Presentation();

        var apiA = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500), Alpha);

        var apiB = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(200), Beta);

        apiA.ManufacturingSourceSiteId.Should().Be(Alpha);
        apiB.ManufacturingSourceSiteId.Should().Be(Beta);
        presentation.Ingredients.Should().HaveCount(2);
    }

    /// <summary>
    /// <b>Absent means nobody has said, never "unsourced".</b> RegOS holds no
    /// provenance for any ingredient recorded before this shipped, and every
    /// ingredient added without one is the ordinary case rather than a gap.
    /// </summary>
    [Fact]
    public void AnIngredientWithNoStatedSourceIsOrdinary()
    {
        var presentation = Presentation();

        var ingredient = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500));

        ingredient.ManufacturingSourceSiteId.Should().BeNull();
    }

    /// <summary>
    /// <b>Dual sourcing is a restate, and the substance does not move.</b> One
    /// API with two qualified suppliers changes where it comes from without
    /// changing what it is — which is exactly why the source lives on the
    /// ingredient rather than on the substance it points at.
    /// </summary>
    [Fact]
    public void ARestateMovesTheSourceAndKeepsTheSubstance()
    {
        var presentation = Presentation();
        var substanceId = SubstanceId.New();

        var ingredient = presentation.AddIngredient(
            substanceId, IngredientRole.Active, Mg(500), Alpha);

        presentation.RestateIngredient(
            ingredient.Id, IngredientRole.Active, Mg(500), Beta);

        var corrected = presentation.Ingredients.Single();

        corrected.SubstanceId.Should().Be(substanceId);
        corrected.ManufacturingSourceSiteId.Should().Be(Beta);
    }

    /// <summary>
    /// <b>Null on a restate means "we no longer say", and it is allowed.</b>
    /// The parameter has no default precisely so that reaching this state is a
    /// choice somebody made rather than an argument they forgot.
    /// </summary>
    [Fact]
    public void ARestateMayWithdrawTheSourceDeliberately()
    {
        var presentation = Presentation();

        var ingredient = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Active, Mg(500), Alpha);

        presentation.RestateIngredient(
            ingredient.Id, IngredientRole.Active, Mg(500), null);

        presentation.Ingredients.Single()
            .ManufacturingSourceSiteId.Should().BeNull();
    }

    /// <summary>
    /// <b>The compiler enforces the deliberate part</b>, which is the whole
    /// reason this parameter breaks the file's own convention of defaulting
    /// optional arguments: a defaulted null would erase provenance for any
    /// caller that had not thought about it.
    /// </summary>
    [Fact]
    public void RestatingTakesNoDefaultForTheSource()
    {
        var restate = typeof(PharmaceuticalProductDetail)
            .GetMethod(nameof(PharmaceuticalProductDetail.RestateIngredient))!;

        var source = restate.GetParameters()
            .Single(x => x.Name == "manufacturingSourceSiteId");

        source.HasDefaultValue.Should().BeFalse();

        // And adding one does default, because an ingredient recorded without
        // provenance is ordinary rather than a loss.
        var add = typeof(PharmaceuticalProductDetail)
            .GetMethod(nameof(PharmaceuticalProductDetail.AddIngredient))!;

        add.GetParameters()
            .Single(x => x.Name == "manufacturingSourceSiteId")
            .HasDefaultValue.Should().BeTrue();
    }

    /// <summary>
    /// <b>An excipient may be sourced too.</b> The rule that binds a strength to
    /// an active does not extend here: where a substance comes from is a fact
    /// about supply, and a preservative has a supplier like anything else.
    /// </summary>
    [Fact]
    public void AnExcipientMayNameASourceWithoutNamingAStrength()
    {
        var presentation = Presentation();

        var excipient = presentation.AddIngredient(
            SubstanceId.New(), IngredientRole.Excipient, null, Beta);

        excipient.Strength.Should().BeNull();
        excipient.ManufacturingSourceSiteId.Should().Be(Beta);
    }
}
