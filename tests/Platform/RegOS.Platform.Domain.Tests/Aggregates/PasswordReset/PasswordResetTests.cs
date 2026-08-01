using FluentAssertions;

using RegOS.Platform.Domain.Aggregates.PasswordReset;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;
using RegOS.Platform.Contracts;

using PasswordResetAggregate =
    RegOS.Platform.Domain.Aggregates.PasswordReset.PasswordReset;

namespace RegOS.Platform.Domain.Tests.Aggregates.PasswordReset;

public sealed class PasswordResetTests
{
    private static readonly DateTime Now =
        new(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc);

    private static PasswordResetAggregate Issue(DateTime? expiresAt = null) =>
        PasswordResetAggregate.Issue(
            UserId.New(),
            "A-HASH",
            expiresAt ?? Now.AddHours(1),
            Now);

    // ---------------------------------------------------------------- issuing

    [Fact]
    public void Is_usable_when_issued()
    {
        var reset = Issue();

        reset.IsUsableAt(Now).Should().BeTrue();
        reset.ConsumedOn.Should().BeNull();
        reset.RevokedOn.Should().BeNull();
        reset.CreatedOn.Should().Be(Now);
    }

    [Fact]
    public void Belongs_to_the_user_it_was_issued_for()
    {
        var user = UserId.New();

        var reset = PasswordResetAggregate.Issue(
            user, "A-HASH", Now.AddHours(1), Now);

        reset.UserId.Should().Be(user);
    }

    [Fact]
    public void Cannot_be_issued_without_a_user()
    {
        var act = () => PasswordResetAggregate.Issue(
            null!, "A-HASH", Now.AddHours(1), Now);

        act.Should().Throw<DomainException>()
            .WithMessage(PasswordResetErrors.UserRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cannot_be_issued_without_a_token_hash(string? hash)
    {
        var act = () => PasswordResetAggregate.Issue(
            UserId.New(), hash!, Now.AddHours(1), Now);

        act.Should().Throw<DomainException>()
            .WithMessage(PasswordResetErrors.TokenHashRequired);
    }

    [Fact]
    public void Cannot_be_issued_already_expired()
    {
        // A reset that is dead on arrival is a bug in whatever issued it, not a
        // link the user should be sent and then refused.
        var act = () => PasswordResetAggregate.Issue(
            UserId.New(), "A-HASH", Now, Now);

        act.Should().Throw<DomainException>()
            .WithMessage(PasswordResetErrors.ExpiryMustBeInTheFuture);
    }

    // --------------------------------------------------------------- expiry

    [Fact]
    public void Is_not_usable_once_expired()
    {
        var reset = Issue(Now.AddHours(1));

        reset.IsUsableAt(Now.AddHours(1).AddSeconds(1)).Should().BeFalse();
    }

    [Fact]
    public void Is_not_usable_at_the_exact_moment_of_expiry()
    {
        // Stated explicitly because "expires at noon" has two readings and only
        // one of them is safe.
        var reset = Issue(Now.AddHours(1));

        reset.IsUsableAt(Now.AddHours(1)).Should().BeFalse();
    }

    [Fact]
    public void Cannot_be_consumed_once_expired()
    {
        var reset = Issue(Now.AddHours(1));

        var act = () => reset.Consume(Now.AddHours(2));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PasswordResetErrors.NoLongerUsable);

        reset.ConsumedOn.Should().BeNull();
    }

    // -------------------------------------------------------------- consuming

    [Fact]
    public void Records_when_it_was_consumed()
    {
        var reset = Issue();

        reset.Consume(Now.AddMinutes(5));

        reset.ConsumedOn.Should().Be(Now.AddMinutes(5));
        reset.IsUsableAt(Now.AddMinutes(5)).Should().BeFalse();
    }

    [Fact]
    public void Cannot_be_consumed_twice()
    {
        // The single-use rule, which is the whole reason this aggregate exists
        // rather than a flag on the user.
        var reset = Issue();

        reset.Consume(Now.AddMinutes(5));

        var act = () => reset.Consume(Now.AddMinutes(6));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PasswordResetErrors.NoLongerUsable);

        reset.ConsumedOn.Should().Be(Now.AddMinutes(5));
    }

    [Fact]
    public void Cannot_be_consumed_once_revoked()
    {
        var reset = Issue();

        reset.Revoke(Now.AddMinutes(1));

        var act = () => reset.Consume(Now.AddMinutes(2));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(PasswordResetErrors.NoLongerUsable);

        reset.ConsumedOn.Should().BeNull();
    }

    // -------------------------------------------------------------- revoking

    [Fact]
    public void Records_when_it_was_revoked()
    {
        var reset = Issue();

        reset.Revoke(Now.AddMinutes(1));

        reset.RevokedOn.Should().Be(Now.AddMinutes(1));
        reset.IsUsableAt(Now.AddMinutes(1)).Should().BeFalse();
    }

    [Fact]
    public void Revoking_twice_keeps_the_first_time()
    {
        // Issuing a replacement withdraws whatever came before without asking
        // what state it was in, so this must be safe to repeat.
        var reset = Issue();

        reset.Revoke(Now.AddMinutes(1));
        reset.Revoke(Now.AddMinutes(9));

        reset.RevokedOn.Should().Be(Now.AddMinutes(1));
    }

    [Fact]
    public void Revoking_a_consumed_reset_leaves_it_alone()
    {
        // A spent reset is history. Marking it revoked afterwards would erase
        // the record of the password having actually been changed.
        var reset = Issue();

        reset.Consume(Now.AddMinutes(5));
        reset.Revoke(Now.AddMinutes(6));

        reset.ConsumedOn.Should().Be(Now.AddMinutes(5));
        reset.RevokedOn.Should().BeNull();
    }
}
