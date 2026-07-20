using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

using ProductAggregate = RegOS.Product.Domain.Product.Product;

namespace RegOS.Product.Domain.Tests.Product;

public sealed class ProductTests
{
    private static readonly OrganizationId Owner = OrganizationId.New();

    [Fact]
    public void Register_starts_the_product_as_registered()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        product.Status.Should().Be(ProductStatus.Registered);
        product.OrganizationId.Should().Be(Owner);
        product.Code.Value.Should().Be("OZE-1");
        product.Name.Value.Should().Be("Ozempic");
        product.Type.Should().Be(ProductType.Drug);
        product.Id.Should().NotBeNull();
    }

    [Fact]
    public void Register_normalizes_the_code()
    {
        ProductAggregate.Register(Owner, " oze-1 ", "Ozempic", ProductType.Drug)
            .Code.Value.Should().Be("OZE-1");
    }

    [Fact]
    public void Register_rejects_a_missing_code()
    {
        var act = () => ProductAggregate.Register(Owner, "  ", "Ozempic", ProductType.Drug);

        act.Should().Throw<DomainException>()
            .WithMessage(ProductErrors.CodeRequired);
    }

    [Fact]
    public void Register_normalizes_the_name()
    {
        ProductAggregate.Register(Owner, "OZE-1", "  Ozempic  ", ProductType.Drug)
            .Name.Value.Should().Be("Ozempic");
    }

    [Fact]
    public void Register_rejects_a_missing_name()
    {
        var act = () => ProductAggregate.Register(Owner, "OZE-1", "  ", ProductType.Drug);

        act.Should().Throw<DomainException>()
            .WithMessage(ProductErrors.NameRequired);
    }

    [Fact]
    public void Rename_replaces_the_name()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        product.Rename("Wegovy");

        product.Name.Value.Should().Be("Wegovy");
    }

    [Fact]
    public void Rename_rejects_a_missing_name_and_leaves_the_product_unchanged()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        var act = () => product.Rename(null);

        act.Should().Throw<DomainException>();
        product.Name.Value.Should().Be("Ozempic");
    }

    [Fact]
    public void Archive_marks_the_product_archived()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        product.Archive();

        product.Status.Should().Be(ProductStatus.Archived);
    }

    [Fact]
    public void Archive_rejects_a_second_attempt()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        product.Archive();
        var act = () => product.Archive();

        // A one-way lifecycle transition, not a toggle: a repeat means the
        // caller is acting on a stale view, and succeeding would hide that.
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProductErrors.AlreadyArchived);
        product.Status.Should().Be(ProductStatus.Archived);
    }

    [Fact]
    public void Archive_preserves_the_product_because_it_is_not_a_deletion()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        product.Archive();

        product.Code.Value.Should().Be("OZE-1");
        product.Name.Value.Should().Be("Ozempic");
        product.Type.Should().Be(ProductType.Drug);
        product.OrganizationId.Should().Be(Owner);
    }

    [Fact]
    public void Two_registrations_are_distinct_products()
    {
        var first = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);
        var second = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        first.Should().NotBe(second);
    }

    [Fact]
    public void ChangeType_reclassifies_the_product()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        product.ChangeType(ProductType.Biologic);

        product.Type.Should().Be(ProductType.Biologic);
    }

    [Fact]
    public void ChangeType_leaves_the_code_and_owner_untouched()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        product.ChangeType(ProductType.Biologic);
        product.Rename("Wegovy");

        // The code identifies the product within its organization; nothing on
        // the update path may change it or move the product between tenants.
        product.Code.Value.Should().Be("OZE-1");
        product.OrganizationId.Should().Be(Owner);
    }

    [Fact]
    public void Updating_does_not_change_status()
    {
        var product = ProductAggregate.Register(Owner, "OZE-1", "Ozempic", ProductType.Drug);

        product.Rename("Wegovy");
        product.ChangeType(ProductType.Biologic);

        // Lifecycle is a separate capability - Archive - not a side effect of
        // editing descriptive fields.
        product.Status.Should().Be(ProductStatus.Registered);
    }
}
