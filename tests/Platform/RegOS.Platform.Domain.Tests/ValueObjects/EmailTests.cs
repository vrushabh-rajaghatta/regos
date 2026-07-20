using FluentAssertions;

using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidAddress_Succeeds()
    {
        var email = Email.Create("john.doe@example.com");

        email.Value.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void Create_TrimsSurroundingWhitespace()
    {
        var email = Email.Create("  john.doe@example.com  ");

        email.Value.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void Create_NormalizesToLowercase()
    {
        var email = Email.Create("John.Doe@Example.COM");

        email.Value.Should().Be("john.doe@example.com");
    }

    [Theory]
    [InlineData("john.doeexample.com")]  // missing '@'
    [InlineData("john@doe@example.com")] // more than one '@'
    [InlineData("@example.com")]         // empty local part
    [InlineData("john@")]                // empty domain part
    [InlineData("john doe@example.com")] // whitespace inside the address
    public void Create_WithMalformedAddress_ThrowsDomainException(string value)
    {
        var act = () => Email.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithMissingValue_ThrowsDomainException(string? value)
    {
        var act = () => Email.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equality_IgnoresCaseAndSurroundingWhitespace()
    {
        Email.Create("John@Example.com")
            .Should().Be(Email.Create("  john@example.com  "));
    }

    [Fact]
    public void Equality_Operator_ComparesByNormalizedValue()
    {
        (Email.Create("john@example.com") == Email.Create("JOHN@EXAMPLE.COM"))
            .Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentAddresses_AreNotEqual()
    {
        Email.Create("john@example.com")
            .Should().NotBe(Email.Create("jane@example.com"));
    }
}
