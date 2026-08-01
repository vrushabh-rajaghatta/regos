using FluentAssertions;

using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;

using SessionAggregate = RegOS.Platform.Domain.Aggregates.Session.Session;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Domain.Tests.Aggregates.Session;

public sealed class SessionTests
{
    private static readonly DateTime Now =
        new(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc);

    private static SessionAggregate Start(
        string? userAgent = "Mozilla/5.0", DateTime? expiresAt = null) =>
        SessionAggregate.Start(
            UserId.New(), userAgent, "203.0.113.7",
            expiresAt ?? Now.AddDays(14), Now);

    [Fact]
    public void Is_active_when_started()
    {
        var session = Start();

        session.IsActiveAt(Now).Should().BeTrue();
        session.CreatedOn.Should().Be(Now);
        session.LastUsedOn.Should().Be(Now);
    }

    [Fact]
    public void Keeps_the_device_context_it_was_given()
    {
        var session = Start();

        session.UserAgent.Should().Be("Mozilla/5.0");
        session.CreatedFromIp.Should().Be("203.0.113.7");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Treats_an_absent_user_agent_as_absent(string? userAgent)
    {
        // A non-browser client legitimately has none, and refusing it a session
        // would break every curl and .http call.
        Start(userAgent).UserAgent.Should().BeNull();
    }

    [Fact]
    public void Truncates_an_overlong_user_agent_rather_than_refusing_it()
    {
        // What a client calls itself is its business. It is not a reason to
        // deny someone a sign-in, but it is also not a scratch pad.
        var session = Start(new string('x', 900));

        session.UserAgent!.Length.Should().Be(512);
    }

    [Fact]
    public void Cannot_start_without_a_user()
    {
        var act = () => SessionAggregate.Start(
            null!, "agent", "ip", Now.AddDays(1), Now);

        act.Should().Throw<DomainException>()
            .WithMessage(SessionErrors.UserRequired);
    }

    [Fact]
    public void Cannot_start_already_expired()
    {
        var act = () => SessionAggregate.Start(
            UserId.New(), "agent", "ip", Now, Now);

        act.Should().Throw<DomainException>()
            .WithMessage(SessionErrors.ExpiryMustBeInTheFuture);
    }

    // ------------------------------------------------------------ refreshing

    [Fact]
    public void Refreshing_moves_last_used_and_expiry_but_not_identity()
    {
        // The property the whole slice rests on: rotation mints a new token,
        // and the session the user sees stays the same row.
        var session = Start();
        var id = session.Id;

        session.Refreshed(Now.AddDays(15), Now.AddHours(3));

        session.Id.Should().Be(id);
        session.LastUsedOn.Should().Be(Now.AddHours(3));
        session.ExpiresAt.Should().Be(Now.AddDays(15));
        session.CreatedOn.Should().Be(Now);
    }

    [Fact]
    public void Is_not_active_once_expired()
    {
        var session = Start(expiresAt: Now.AddDays(1));

        session.IsActiveAt(Now.AddDays(1).AddSeconds(1)).Should().BeFalse();
    }

    [Fact]
    public void Is_not_active_at_the_exact_moment_of_expiry()
    {
        var session = Start(expiresAt: Now.AddDays(1));

        session.IsActiveAt(Now.AddDays(1)).Should().BeFalse();
    }

    // -------------------------------------------------------------- revoking

    [Fact]
    public void Is_not_active_once_revoked()
    {
        var session = Start();

        session.Revoke(Now.AddHours(1));

        session.IsActiveAt(Now.AddHours(1)).Should().BeFalse();
        session.RevokedOn.Should().Be(Now.AddHours(1));
    }

    [Fact]
    public void Revoking_twice_keeps_the_first_time()
    {
        var session = Start();

        session.Revoke(Now.AddHours(1));
        session.Revoke(Now.AddHours(5));

        session.RevokedOn.Should().Be(Now.AddHours(1));
    }
}
