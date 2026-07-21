using FluentAssertions;

using RegOS.SharedKernel.Primitives;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Domain.Tests.Aggregates;

public class UserTests
{
    private static User NewInvitedUser() =>
        User.CreateForTenant(
            TenantId.New(),
            Email.Create("john.doe@example.com"),
            "John",
            "Doe");

    [Fact]
    public void Create_StartsInInvitedStatus()
    {
        NewInvitedUser().Status.Should().Be(UserStatus.Invited);
    }

    [Fact]
    public void Create_PopulatesAllFields()
    {
        var tenantId = TenantId.New();

        var user = User.CreateForTenant(
            tenantId,
            Email.Create("john.doe@example.com"),
            "  John  ",
            "  Doe  ");

        user.Id.Should().NotBeNull();
        user.TenantId.Should().Be(tenantId);
        user.Email.Value.Should().Be("john.doe@example.com");
        user.FirstName.Should().Be("John");   // trimmed
        user.LastName.Should().Be("Doe");     // trimmed
        user.CreatedOn.Should().NotBe(default);
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("   ", "Doe")]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    public void Create_WithMissingName_ThrowsDomainException(string first, string last)
    {
        var act = () => User.CreateForTenant(
            TenantId.New(),
            Email.Create("john.doe@example.com"),
            first,
            last);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateForTenant_WithNullTenant_ThrowsDomainException()
    {
        var act = () => User.CreateForTenant(
            null!,
            Email.Create("john.doe@example.com"),
            "John",
            "Doe");

        act.Should().Throw<DomainException>()
            .WithMessage(UserErrors.TenantRequired);
    }

    [Fact]
    public void CreatePlatformUser_HasNoTenant()
    {
        var user = User.CreatePlatformUser(
            Email.Create("platform@example.com"),
            "Platform",
            "Administrator");

        user.TenantId.Should().BeNull();
        user.Status.Should().Be(UserStatus.Invited);
    }

    [Fact]
    public void CreatePlatformUser_EnforcesTheSameNameAndEmailInvariants()
    {
        var act = () => User.CreatePlatformUser(
            Email.Create("platform@example.com"),
            "   ",
            "Administrator");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activate_FromInvited_BecomesActive()
    {
        var user = NewInvitedUser();

        user.Activate();

        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Activate_FromInactive_BecomesActive()
    {
        var user = NewInvitedUser();
        user.Deactivate();

        user.Activate();

        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_IsIdempotent()
    {
        var user = NewInvitedUser();
        user.Activate();

        user.Activate();

        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Deactivate_FromInvited_BecomesInactive()
    {
        var user = NewInvitedUser();

        user.Deactivate();

        user.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public void Deactivate_FromActive_BecomesInactive()
    {
        var user = NewInvitedUser();
        user.Activate();

        user.Deactivate();

        user.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_IsIdempotent()
    {
        var user = NewInvitedUser();
        user.Deactivate();

        user.Deactivate();

        user.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public void ChangeName_UpdatesAndTrimsNames()
    {
        var user = NewInvitedUser();

        user.ChangeName("  Jane  ", "  Smith  ");

        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
    }

    [Fact]
    public void ChangeName_WithSameValues_IsNoOp()
    {
        var user = NewInvitedUser();

        user.ChangeName("John", "Doe");

        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
    }

    [Fact]
    public void ChangeName_WithSameValuesButUntrimmed_IsNoOp()
    {
        var user = NewInvitedUser();

        user.ChangeName("  John  ", "  Doe  ");

        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
    }

    [Theory]
    [InlineData("", "Smith")]
    [InlineData("Jane", "   ")]
    public void ChangeName_WithMissingName_ThrowsDomainException(string first, string last)
    {
        var user = NewInvitedUser();

        var act = () => user.ChangeName(first, last);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeEmail_UpdatesEmail()
    {
        var user = NewInvitedUser();

        user.ChangeEmail(Email.Create("jane.smith@example.com"));

        user.Email.Value.Should().Be("jane.smith@example.com");
    }

    [Fact]
    public void ChangeEmail_WithSameNormalizedValue_IsNoOp()
    {
        var user = NewInvitedUser(); // john.doe@example.com

        user.ChangeEmail(Email.Create("JOHN.DOE@Example.com"));

        user.Email.Value.Should().Be("john.doe@example.com");
    }
}
