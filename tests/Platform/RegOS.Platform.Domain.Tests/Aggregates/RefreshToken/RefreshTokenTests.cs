using FluentAssertions;

using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;

using RefreshTokenAggregate =
    RegOS.Platform.Domain.Aggregates.RefreshToken.RefreshToken;

namespace RegOS.Platform.Domain.Tests.Aggregates.RefreshToken;

public sealed class RefreshTokenTests
{
    private static readonly DateTime Now =
        new(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc);

    private static RefreshTokenAggregate Issue(DateTime? expiresAt = null) =>
        RefreshTokenAggregate.Issue(
            UserId.New(),
            SessionId.New(),
            "A-HASH",
            expiresAt ?? Now.AddDays(14),
            Now);

    [Fact]
    public void Is_active_when_issued()
    {
        Issue().IsActiveAt(Now).Should().BeTrue();
    }

    [Fact]
    public void Is_not_active_once_expired()
    {
        var token = Issue(Now.AddDays(1));

        token.IsActiveAt(Now.AddDays(1).AddSeconds(1)).Should().BeFalse();
    }

    [Fact]
    public void Is_not_active_at_the_exact_moment_of_expiry()
    {
        // The boundary is exclusive: a token is dead the instant it expires,
        // not a tick afterwards.
        var expiresAt = Now.AddDays(1);

        Issue(expiresAt).IsActiveAt(expiresAt).Should().BeFalse();
    }

    [Fact]
    public void Is_not_active_once_revoked()
    {
        var token = Issue();

        token.Revoke(Now);

        token.IsActiveAt(Now).Should().BeFalse();
    }

    [Fact]
    public void Records_when_it_was_revoked()
    {
        var token = Issue();

        token.Revoke(Now);

        token.RevokedOn.Should().Be(Now);
    }

    [Fact]
    public void Keeps_the_first_revocation_time_when_revoked_again()
    {
        // Signing out twice must not rewrite history to the later moment.
        var token = Issue();

        token.Revoke(Now);
        token.Revoke(Now.AddHours(1));

        token.RevokedOn.Should().Be(Now);
    }

    [Fact]
    public void Rotating_revokes_it_and_records_the_replacement()
    {
        var token = Issue();
        var replacement = RefreshTokenId.New();

        token.RotateTo(replacement, Now);

        token.IsActiveAt(Now).Should().BeFalse();
        token.ReplacedBy.Should().Be(replacement);
    }

    [Fact]
    public void Has_no_replacement_until_it_is_rotated()
    {
        Issue().ReplacedBy.Should().BeNull();
    }

    [Fact]
    public void Revoking_records_no_replacement()
    {
        // Signing out is not rotation. Recording a replacement here would make
        // a deliberately ended session look like a rotated one.
        var token = Issue();

        token.Revoke(Now);

        token.ReplacedBy.Should().BeNull();
    }

    [Fact]
    public void Rejects_a_missing_user()
    {
        var act = () => RefreshTokenAggregate.Issue(
            null!,
            SessionId.New(), "A-HASH", Now.AddDays(1), Now);

        act.Should().Throw<DomainException>()
            .WithMessage(RefreshTokenErrors.UserRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_hash(string hash)
    {
        var act = () => RefreshTokenAggregate.Issue(
            UserId.New(),
            SessionId.New(), hash, Now.AddDays(1), Now);

        act.Should().Throw<DomainException>()
            .WithMessage(RefreshTokenErrors.TokenHashRequired);
    }

    [Fact]
    public void Rejects_an_expiry_in_the_past()
    {
        var act = () => RefreshTokenAggregate.Issue(
            UserId.New(),
            SessionId.New(), "A-HASH", Now.AddSeconds(-1), Now);

        act.Should().Throw<DomainException>()
            .WithMessage(RefreshTokenErrors.ExpiryMustBeInTheFuture);
    }

    [Fact]
    public void Rejects_a_token_that_expires_the_moment_it_is_created()
    {
        var act = () => RefreshTokenAggregate.Issue(
            UserId.New(),
            SessionId.New(), "A-HASH", Now, Now);

        act.Should().Throw<DomainException>()
            .WithMessage(RefreshTokenErrors.ExpiryMustBeInTheFuture);
    }
}
