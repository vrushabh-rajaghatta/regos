using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

using ProductAggregate = RegOS.Product.Domain.Product.Product;

namespace RegOS.Product.Domain.Tests.Product;

public sealed class ProductTests
{
    [Fact]
    public void Register_starts_the_product_as_registered()
    {
        var product = ProductAggregate.Register("Ozempic", ProductType.Drug);

        product.Status.Should().Be(ProductStatus.Registered);
        product.Name.Value.Should().Be("Ozempic");
        product.Type.Should().Be(ProductType.Drug);
        product.Id.Should().NotBeNull();
    }

    [Fact]
    public void Register_normalizes_the_name()
    {
        ProductAggregate.Register("  Ozempic  ", ProductType.Drug)
            .Name.Value.Should().Be("Ozempic");
    }

    [Fact]
    public void Register_rejects_a_missing_name()
    {
        var act = () => ProductAggregate.Register("  ", ProductType.Drug);

        act.Should().Throw<DomainException>()
            .WithMessage(ProductErrors.NameRequired);
    }

    [Fact]
    public void Rename_replaces_the_name()
    {
        var product = ProductAggregate.Register("Ozempic", ProductType.Drug);

        product.Rename("Wegovy");

        product.Name.Value.Should().Be("Wegovy");
    }

    [Fact]
    public void Rename_rejects_a_missing_name_and_leaves_the_product_unchanged()
    {
        var product = ProductAggregate.Register("Ozempic", ProductType.Drug);

        var act = () => product.Rename(null);

        act.Should().Throw<DomainException>();
        product.Name.Value.Should().Be("Ozempic");
    }

    [Fact]
    public void Archive_marks_the_product_archived()
    {
        var product = ProductAggregate.Register("Ozempic", ProductType.Drug);

        product.Archive();

        product.Status.Should().Be(ProductStatus.Archived);
    }

    [Fact]
    public void Archive_is_idempotent_so_retries_are_safe()
    {
        var product = ProductAggregate.Register("Ozempic", ProductType.Drug);

        product.Archive();
        product.Archive();

        product.Status.Should().Be(ProductStatus.Archived);
    }

    [Fact]
    public void Two_registrations_are_distinct_products()
    {
        var first = ProductAggregate.Register("Ozempic", ProductType.Drug);
        var second = ProductAggregate.Register("Ozempic", ProductType.Drug);

        first.Should().NotBe(second);
    }
}
