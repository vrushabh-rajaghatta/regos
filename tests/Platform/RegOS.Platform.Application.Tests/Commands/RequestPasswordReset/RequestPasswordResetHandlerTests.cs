using FluentAssertions;
using Microsoft.Extensions.Options;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Commands.RequestPasswordReset;
using RegOS.Platform.Application.PasswordResets;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.Platform.Infrastructure.Authentication;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Tests.Commands.RequestPasswordReset;

/// <summary>
/// Almost every test here asserts that <em>nothing happened</em>, which is the
/// point: this endpoint answers identically whoever asks, so the only way to
/// tell the cases apart is from inside.
/// </summary>
public sealed class RequestPasswordResetHandlerTests
{
    private const string Address = "john.doe@example.com";

    private static UserAggregate NewUser() =>
        UserAggregate.Create(
            OrganizationId.New(), Email.Create(Address), "John", "Doe");

    private static UserAggregate ActiveUser()
    {
        var user = NewUser();
        user.Activate();
        return user;
    }

    /// <summary>
    /// The real token issuer, not a fake: it does no I/O, and a fake would only
    /// prove that the fake was called.
    /// </summary>
    private static (RequestPasswordResetHandler Handler,
        FakePasswordResetNotifier Notifier,
        FakePasswordResetRepository Resets) NewHandler(UserAggregate? user)
    {
        var notifier = new FakePasswordResetNotifier();
        var resets = new FakePasswordResetRepository();

        var handler = new RequestPasswordResetHandler(
            new PasswordResetIssuer(
                notifier,
                new PasswordResetTokenIssuer(
                    new SecretTokenFactory(),
                    Options.Create(new PasswordResetOptions { Minutes = 60 })),
                resets),
            new FakeUserRepository(user));

        return (handler, notifier, resets);
    }

    private static Task RequestAsync(
        RequestPasswordResetHandler handler, string? email = Address) =>
        handler.HandleAsync(
            new RequestPasswordResetCommand(email), CancellationToken.None);

    // ------------------------------------------------------------ the one case

    [Fact]
    public async Task Issues_a_reset_for_an_active_user()
    {
        var (handler, notifier, resets) = NewHandler(ActiveUser());

        await RequestAsync(handler);

        notifier.SendCount.Should().Be(1);
        notifier.Token.Should().NotBeNullOrWhiteSpace();
        resets.Added.Should().NotBeNull();

        // The link carries the secret; the store keeps only its hash.
        resets.Added!.TokenHash.Should().NotBe(notifier.Token);
    }

    [Fact]
    public async Task Requesting_again_withdraws_the_previous_link()
    {
        // Otherwise a user who asks twice has two working links in their
        // mailbox and knows about one.
        var (handler, notifier, resets) = NewHandler(ActiveUser());

        await RequestAsync(handler);

        var first = resets.Added!;

        await RequestAsync(handler);

        notifier.SendCount.Should().Be(2);
        first.RevokedOn.Should().NotBeNull();
        resets.Updated.Should().Contain(first);
        resets.Added.Should().NotBe(first);
        resets.Added!.RevokedOn.Should().BeNull();
    }

    // -------------------------------------------------------- the silent cases

    [Fact]
    public async Task Says_nothing_about_an_address_nobody_has()
    {
        var (handler, notifier, resets) = NewHandler(user: null);

        await RequestAsync(handler);

        notifier.SendCount.Should().Be(0);
        resets.Added.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-address")]
    public async Task Says_nothing_about_a_malformed_address(string? email)
    {
        // A 400 here would let a caller tell a well-formed unknown address from
        // a malformed one. Small oracle, still an oracle.
        var (handler, notifier, _) = NewHandler(ActiveUser());

        var act = () => RequestAsync(handler, email);

        await act.Should().NotThrowAsync();
        notifier.SendCount.Should().Be(0);
    }

    [Fact]
    public async Task Says_nothing_to_a_user_who_has_not_accepted_their_invitation()
    {
        // The rule that keeps invitation as the only route to a first
        // credential (ADR-027). Reset recovers a password; it does not create
        // one.
        var (handler, notifier, resets) = NewHandler(NewUser());

        await RequestAsync(handler);

        notifier.SendCount.Should().Be(0);
        resets.Added.Should().BeNull();
    }

    [Fact]
    public async Task Says_nothing_to_a_deactivated_user()
    {
        var user = ActiveUser();
        user.Deactivate();

        var (handler, notifier, resets) = NewHandler(user);

        await RequestAsync(handler);

        notifier.SendCount.Should().Be(0);
        resets.Added.Should().BeNull();
    }
}
