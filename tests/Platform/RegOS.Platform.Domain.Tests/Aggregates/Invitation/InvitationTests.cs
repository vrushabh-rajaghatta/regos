using FluentAssertions;

using RegOS.Platform.Domain.Aggregates.Invitation;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;

using InvitationAggregate =
    RegOS.Platform.Domain.Aggregates.Invitation.Invitation;

namespace RegOS.Platform.Domain.Tests.Aggregates.Invitation;

public sealed class InvitationTests
{
    private static readonly DateTime Now =
        new(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc);

    private static InvitationAggregate Issue(DateTime? expiresAt = null) =>
        InvitationAggregate.Issue(
            UserId.New(), "A-HASH", expiresAt ?? Now.AddDays(7), Now);

    [Fact]
    public void Is_pending_when_issued() =>
        Issue().IsPendingAt(Now).Should().BeTrue();

    [Fact]
    public void Is_not_pending_once_expired() =>
        Issue(Now.AddDays(7)).IsPendingAt(Now.AddDays(8)).Should().BeFalse();

    [Fact]
    public void Is_not_pending_at_the_exact_moment_of_expiry()
    {
        var expiresAt = Now.AddDays(7);

        Issue(expiresAt).IsPendingAt(expiresAt).Should().BeFalse();
    }

    [Fact]
    public void Is_not_pending_once_consumed()
    {
        var invitation = Issue();

        invitation.Consume(Now);

        invitation.IsPendingAt(Now).Should().BeFalse();
    }

    [Fact]
    public void Is_not_pending_once_revoked()
    {
        var invitation = Issue();

        invitation.Revoke(Now);

        invitation.IsPendingAt(Now).Should().BeFalse();
    }

    [Fact]
    public void Cannot_be_consumed_twice()
    {
        // Pending or finished, never both. Two acceptances racing must not both
        // believe they won.
        var invitation = Issue();

        invitation.Consume(Now);

        var act = () => invitation.Consume(Now);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(InvitationErrors.NotPending);
    }

    [Fact]
    public void Cannot_be_consumed_after_expiry()
    {
        var invitation = Issue(Now.AddDays(7));

        var act = () => invitation.Consume(Now.AddDays(8));

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Cannot_be_consumed_after_revocation()
    {
        var invitation = Issue();

        invitation.Revoke(Now);

        var act = () => invitation.Consume(Now);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Revoking_a_consumed_invitation_leaves_it_consumed()
    {
        // It was used. Recording it as revoked would erase that.
        var invitation = Issue();

        invitation.Consume(Now);
        invitation.Revoke(Now.AddHours(1));

        invitation.ConsumedOn.Should().Be(Now);
        invitation.RevokedOn.Should().BeNull();
    }

    [Fact]
    public void Keeps_the_first_revocation_time_when_revoked_again()
    {
        var invitation = Issue();

        invitation.Revoke(Now);
        invitation.Revoke(Now.AddHours(1));

        invitation.RevokedOn.Should().Be(Now);
    }

    [Fact]
    public void Rejects_a_missing_user()
    {
        var act = () => InvitationAggregate.Issue(
            null!, "A-HASH", Now.AddDays(1), Now);

        act.Should().Throw<DomainException>()
            .WithMessage(InvitationErrors.UserRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_hash(string hash)
    {
        var act = () => InvitationAggregate.Issue(
            UserId.New(), hash, Now.AddDays(1), Now);

        act.Should().Throw<DomainException>()
            .WithMessage(InvitationErrors.TokenHashRequired);
    }

    [Fact]
    public void Rejects_an_expiry_that_is_not_in_the_future()
    {
        var act = () => InvitationAggregate.Issue(
            UserId.New(), "A-HASH", Now, Now);

        act.Should().Throw<DomainException>()
            .WithMessage(InvitationErrors.ExpiryMustBeInTheFuture);
    }
}
