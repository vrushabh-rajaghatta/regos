using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Tests.Product;

public sealed class ProductIdTests
{
    [Fact]
    public void Rejects_an_empty_guid()
    {
        var act = () => new ProductId(Guid.Empty);

        // A DomainException, not an ArgumentException: an all-zero guid comes
        // from the caller, so it is a 400 rather than a 500.
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_another_id_with_the_same_value()
    {
        var value = Guid.NewGuid();

        ProductId.From(value).Should().Be(ProductId.From(value));
    }

    [Fact]
    public void Is_never_equal_to_a_different_id_type_wrapping_the_same_guid()
    {
        var value = Guid.NewGuid();

        ProductId.From(value).Equals(OrganizationId.From(value))
            .Should().BeFalse();
    }

    [Fact]
    public void New_produces_distinct_ids()
    {
        ProductId.New().Should().NotBe(ProductId.New());
    }
}
