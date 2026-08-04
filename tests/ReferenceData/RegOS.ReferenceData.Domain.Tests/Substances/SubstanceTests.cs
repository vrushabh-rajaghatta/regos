using FluentAssertions;

using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.Tests.Substances;

public class SubstanceTests
{
    private static readonly CodedConcept Chemical =
        CodedConcept.Internal("CHEMICAL", "Chemical");

    private static readonly CodedConcept Synthetic =
        CodedConcept.Internal("SYNTHETIC", "Synthetic");

    private static Substance TenantSubstance(
        string name = "Compound-X", string? inn = null)
        => Substance.CreateForTenant(
            TenantId.From(Guid.NewGuid()), name, inn, Chemical, Synthetic);

    private static Substance SharedSubstance(string name = "Paracetamol")
        => Substance.Seed(
            SubstanceId.New(), name, name, Chemical, Synthetic);

    [Fact]
    public void ASeededSubstanceBelongsToNobody()
    {
        var substance = SharedSubstance();

        substance.TenantId.Should().BeNull();
        substance.IsShared.Should().BeTrue();
    }

    [Fact]
    public void ATenantsSubstanceIsStampedWithTheirTenant()
    {
        var tenantId = TenantId.From(Guid.NewGuid());

        var substance = Substance.CreateForTenant(
            tenantId, "Compound-X", null, Chemical, Synthetic);

        substance.TenantId.Should().Be(tenantId);
        substance.IsShared.Should().BeFalse();
    }

    /// <summary>
    /// The tenant half of the aggregate cannot produce a shared row. A tenant's
    /// write path calls this factory and nothing else, so "a tenant may never
    /// create a platform substance" is the absence of a way rather than a
    /// check (ADR-058 §2).
    /// </summary>
    [Fact]
    public void ATenantSubstanceWithNoTenantIsRefused()
    {
        var act = () => Substance.CreateForTenant(
            null!, "Compound-X", null, Chemical, Synthetic);

        act.Should().Throw<DomainException>()
            .WithMessage(SubstanceErrors.TenantRequired);
    }

    /// <summary>
    /// The case the tenant half exists to serve: an innovator holds a compound
    /// before anyone has assigned it an INN, and the field's absence is the
    /// fact being recorded (EPIC-010a D7).
    /// </summary>
    [Fact]
    public void AProprietaryCompoundNeedsNoInn()
    {
        var substance = TenantSubstance("RGX-1174");

        substance.Inn.Should().BeNull();
    }

    [Fact]
    public void APreferredNameAndAnInnAreTwoFacts()
    {
        var substance = SharedSubstance("Aspirin");

        substance.Name.Should().Be("Aspirin");

        Substance.Seed(
                SubstanceId.New(),
                "Aspirin",
                "Acetylsalicylic acid",
                Chemical,
                Synthetic)
            .Inn.Should().Be("Acetylsalicylic acid");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ANameIsRequired(string? name)
    {
        var act = () => TenantSubstance(name!);

        act.Should().Throw<DomainException>()
            .WithMessage(SubstanceErrors.NameRequired);
    }

    [Fact]
    public void ANameIsTrimmed()
    {
        TenantSubstance("  Compound-X  ").Name.Should().Be("Compound-X");
    }

    [Fact]
    public void ANameTooLongIsRefused()
    {
        var act = () => TenantSubstance(new string('x', Substance.NameMaxLength + 1));

        act.Should().Throw<DomainException>()
            .WithMessage(SubstanceErrors.NameTooLong);
    }

    [Fact]
    public void AClassIsRequired()
    {
        var act = () => Substance.CreateForTenant(
            TenantId.From(Guid.NewGuid()), "Compound-X", null, null!, Synthetic);

        act.Should().Throw<DomainException>()
            .WithMessage(SubstanceErrors.ClassRequired);
    }

    [Fact]
    public void ATypeIsRequired()
    {
        var act = () => Substance.CreateForTenant(
            TenantId.From(Guid.NewGuid()), "Compound-X", null, Chemical, null!);

        act.Should().Throw<DomainException>()
            .WithMessage(SubstanceErrors.TypeRequired);
    }

    /// <summary>
    /// A blank optional identifier is stored as absent. An empty CAS number and
    /// a missing one are the same fact, and only one of them can be queried
    /// for.
    /// </summary>
    [Fact]
    public void BlankOptionalIdentifiersCollapseToNull()
    {
        var substance = Substance.CreateForTenant(
            TenantId.From(Guid.NewGuid()),
            "Compound-X",
            inn: "   ",
            Chemical,
            Synthetic,
            casNumber: "",
            uniiCode: "  ",
            molecularFormula: "",
            description: "   ");

        substance.Inn.Should().BeNull();
        substance.CasNumber.Should().BeNull();
        substance.UniiCode.Should().BeNull();
        substance.MolecularFormula.Should().BeNull();
        substance.Description.Should().BeNull();
    }

    /// <summary>
    /// The GSRS seam. A seeded row cannot carry a UNII, because RegOS does not
    /// hold GSRS and a populated code would claim it does (ADR-058 §6) — while
    /// a tenant that knows its own compound's code may still record it.
    /// </summary>
    [Fact]
    public void ASeededSubstanceCarriesNoUnii()
    {
        SharedSubstance().UniiCode.Should().BeNull();
    }

    [Fact]
    public void ATenantMayRecordItsOwnUnii()
    {
        var substance = Substance.CreateForTenant(
            TenantId.From(Guid.NewGuid()),
            "Compound-X",
            null,
            Chemical,
            Synthetic,
            uniiCode: "362O9ITL9D");

        substance.UniiCode.Should().Be("362O9ITL9D");
    }

    /// <summary>
    /// A seeded id is deterministic, so re-running the initializer against a
    /// database that already has the row inserts nothing.
    /// </summary>
    [Fact]
    public void ASeededSubstanceKeepsTheIdItWasGiven()
    {
        var id = SubstanceId.New();

        Substance.Seed(id, "Paracetamol", "Paracetamol", Chemical, Synthetic)
            .Id.Should().Be(id);
    }
}
