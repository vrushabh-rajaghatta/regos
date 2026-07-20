using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Tests.Product;

public sealed class ProductNameTests
{
    [Fact]
    public void Trims_leading_and_trailing_whitespace()
    {
        var name = ProductName.Create("   Infusion Pump   ");

        name.Value.Should().Be("Infusion Pump");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_name(string? value)
    {
        var act = () => ProductName.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage(ProductErrors.NameRequired);
    }

    [Fact]
    public void Rejects_a_name_longer_than_the_maximum()
    {
        var act = () => ProductName.Create(new string('a', ProductName.MaxLength + 1));

        act.Should().Throw<DomainException>()
            .WithMessage(ProductErrors.NameTooLong);
    }

    [Fact]
    public void Accepts_a_name_of_exactly_the_maximum_length()
    {
        var value = new string('a', ProductName.MaxLength);

        ProductName.Create(value).Value.Should().Be(value);
    }

    [Fact]
    public void Measures_length_after_trimming()
    {
        // The padding must not count towards the limit, otherwise a name the
        // user considers valid is rejected for invisible characters.
        var value = "  " + new string('a', ProductName.MaxLength) + "  ";

        ProductName.Create(value).Value.Should().HaveLength(ProductName.MaxLength);
    }

    [Fact]
    public void Equals_another_name_with_the_same_value()
    {
        ProductName.Create("Ozempic").Should().Be(ProductName.Create(" Ozempic "));
    }
}
