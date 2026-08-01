using FluentAssertions;

using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.SharedKernel.Exceptions;
using RegOS.Platform.Contracts;

using UserCredentialAggregate =
    RegOS.Platform.Domain.Aggregates.UserCredential.UserCredential;

namespace RegOS.Platform.Domain.Tests.UserCredential;

public sealed class UserCredentialTests
{
    private static readonly DateTime Created =
        new(2026, 7, 21, 9, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime Later =
        new(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc);

    private static UserCredentialAggregate Existing() =>
        UserCredentialAggregate.Create(UserId.New(), "hash-one", Created);

    [Fact]
    public void Is_identified_by_its_user()
    {
        // The key is the UserId, which is what makes "at most one credential
        // per user" a property of the type rather than a rule to remember.
        var userId = UserId.New();

        var credential = UserCredentialAggregate.Create(
            userId, "hash", Created);

        credential.Id.Should().Be(userId);
    }

    [Fact]
    public void Stores_the_hash_it_is_given()
    {
        var credential = UserCredentialAggregate.Create(
            UserId.New(), "opaque-hash", Created);

        credential.PasswordHash.Should().Be("opaque-hash");
    }

    [Fact]
    public void Starts_with_matching_timestamps()
    {
        var credential = Existing();

        credential.CreatedOn.Should().Be(Created);
        credential.UpdatedOn.Should().Be(Created);
    }

    [Fact]
    public void Rejects_a_credential_with_no_user()
    {
        var act = () => UserCredentialAggregate.Create(null!, "hash", Created);

        act.Should().Throw<DomainException>()
            .WithMessage(UserCredentialErrors.UserRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_hash(string? hash)
    {
        var act = () => UserCredentialAggregate.Create(
            UserId.New(), hash!, Created);

        act.Should().Throw<DomainException>()
            .WithMessage(UserCredentialErrors.PasswordHashRequired);
    }

    [Fact]
    public void Replaces_the_hash_when_the_password_changes()
    {
        var credential = Existing();

        credential.ChangePassword("hash-two", Later);

        credential.PasswordHash.Should().Be("hash-two");
    }

    [Fact]
    public void Records_when_the_password_last_changed()
    {
        var credential = Existing();

        credential.ChangePassword("hash-two", Later);

        credential.UpdatedOn.Should().Be(Later);
        credential.CreatedOn.Should().Be(Created);
    }

    [Fact]
    public void Rejects_changing_to_a_missing_hash()
    {
        var credential = Existing();

        var act = () => credential.ChangePassword("  ", Later);

        act.Should().Throw<DomainException>()
            .WithMessage(UserCredentialErrors.PasswordHashRequired);
    }

    [Fact]
    public void Never_exposes_a_plaintext_password()
    {
        // The aggregate has no property, constructor or method that accepts or
        // returns a Password. If this ever fails to compile-by-inspection, the
        // hashing boundary has leaked into the domain.
        typeof(UserCredentialAggregate)
            .GetProperties()
            .Should()
            .NotContain(property => property.PropertyType.Name == "Password");
    }
}
