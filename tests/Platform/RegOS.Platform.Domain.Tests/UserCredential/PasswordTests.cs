using FluentAssertions;

using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Tests.UserCredential;

public sealed class PasswordTests
{
    [Fact]
    public void Accepts_a_password_at_the_minimum_length()
    {
        var password = Password.Create(new string('a', Password.MinimumLength));

        password.Value.Should().HaveLength(Password.MinimumLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_password(string? value)
    {
        var act = () => Password.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage(PasswordErrors.Required);
    }

    [Fact]
    public void Rejects_a_password_shorter_than_the_minimum()
    {
        var act = () => Password.Create(new string('a', Password.MinimumLength - 1));

        act.Should().Throw<DomainException>()
            .WithMessage(PasswordErrors.TooShort);
    }

    [Fact]
    public void Rejects_a_password_longer_than_the_maximum()
    {
        // A length guard, not a strength rule: hashing unbounded input on an
        // unauthenticated endpoint is free work for an attacker.
        var act = () => Password.Create(new string('a', Password.MaximumLength + 1));

        act.Should().Throw<DomainException>()
            .WithMessage(PasswordErrors.TooLong);
    }

    [Fact]
    public void Preserves_surrounding_whitespace()
    {
        // Unlike Email, a password is not trimmed: spaces are legitimate
        // characters, and removing them would reject a password the user
        // successfully set.
        var password = Password.Create("  spaced  ");

        password.Value.Should().Be("  spaced  ");
    }

    [Fact]
    public void Preserves_the_password_exactly()
    {
        const string raw = "Correct horse battery staple";

        Password.Create(raw).Value.Should().Be(raw);
    }
}
