using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Domain.Tests.Product;

public sealed class ProductCodeTests
{
    [Fact]
    public void Normalizes_to_upper_case()
    {
        ProductCode.Create("abc-123").Value.Should().Be("ABC-123");
    }

    [Fact]
    public void Trims_surrounding_whitespace()
    {
        ProductCode.Create("  ACE-500  ").Value.Should().Be("ACE-500");
    }

    [Fact]
    public void Treats_codes_differing_only_by_case_as_equal()
    {
        // This is what stops the same product being registered twice: the
        // uniqueness check compares normalized values.
        ProductCode.Create("abc-123").Should().Be(ProductCode.Create("ABC-123"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_code(string? value)
    {
        var act = () => ProductCode.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage(ProductErrors.CodeRequired);
    }

    [Fact]
    public void Rejects_a_code_longer_than_the_maximum()
    {
        var act = () => ProductCode.Create(new string('A', ProductCode.MaxLength + 1));

        act.Should().Throw<DomainException>()
            .WithMessage(ProductErrors.CodeTooLong);
    }

    [Theory]
    [InlineData("ABC 123")]
    [InlineData("ABC/123")]
    [InlineData("ABC.123")]
    [InlineData("ABC#123")]
    public void Rejects_characters_outside_the_permitted_set(string value)
    {
        var act = () => ProductCode.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage(ProductErrors.CodeInvalidCharacters);
    }

    [Theory]
    [InlineData("ACE-500")]
    [InlineData("OZE_1")]
    [InlineData("ABC123")]
    public void Accepts_letters_digits_dashes_and_underscores(string value)
    {
        ProductCode.Create(value).Value.Should().Be(value);
    }
}
