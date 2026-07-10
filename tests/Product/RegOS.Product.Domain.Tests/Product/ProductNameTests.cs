using FluentAssertions;

namespace RegOS.Product.Domain.Tests.Product;

public class ProductNameTests
{
    [Fact]
    public void Should_Trim_Leading_And_Trailing_Whitespace()
    {
        // Arrange
        var value = "   Infusion Pump   ";

        // Act
        var productName = new ProductName(value);

        // Assert
        productName.Value.Should().Be("Infusion Pump");
    }
}