using FluentAssertions;

using RegOS.Registration.Application.Queries;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Tests;

/// <summary>
/// Expiry proximity: derived on every read, never stored, and never a judgement
/// about what counts as urgent.
/// </summary>
/// <remarks>
/// A pure unit test, unlike the rest of this project — the derivation touches no
/// database. It lives here because it is application-layer code, and the project
/// is named for the layer rather than the style.
/// </remarks>
public class ExpiryVisibilityTests
{
    private static readonly DateOnly Today = new(2026, 7, 31);

    [Fact]
    public void AnApprovedRegistrationCountsDownToItsExpiry()
    {
        var facts = ExpiryVisibility.For(
            RegistrationStatus.Approved, new DateOnly(2026, 8, 30), Today);

        facts.HasRunningValidity.Should().BeTrue();
        facts.DaysUntilExpiry.Should().Be(30);
        facts.IsExpired.Should().BeFalse();
    }

    /// <summary>
    /// The strongest attention signal the system has: lapsed in the world, not
    /// yet recorded here. Clamping it to zero would discard exactly the
    /// information worth surfacing.
    /// </summary>
    [Fact]
    public void APassedExpiryReportsHowLongAgoItLapsed()
    {
        var facts = ExpiryVisibility.For(
            RegistrationStatus.Approved, new DateOnly(2026, 6, 30), Today);

        facts.DaysUntilExpiry.Should().Be(-31);
        facts.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void ExpiringTodayIsNotYetExpired()
    {
        var facts = ExpiryVisibility.For(
            RegistrationStatus.Approved, Today, Today);

        facts.DaysUntilExpiry.Should().Be(0);
        facts.IsExpired.Should().BeFalse();
    }

    /// <summary>
    /// A surrendered authorisation still carries the expiry date it was granted
    /// with, but it is no longer on the validity timeline. Reporting a countdown
    /// for it would not be noise — it would be false.
    /// </summary>
    [Theory]
    [InlineData(RegistrationStatus.Withdrawn)]
    [InlineData(RegistrationStatus.Expired)]
    [InlineData(RegistrationStatus.Refused)]
    public void ATerminalRegistrationIsNoLongerCountingDown(
        RegistrationStatus status)
    {
        var facts = ExpiryVisibility.For(
            status, new DateOnly(2031, 2, 8), Today);

        facts.HasRunningValidity.Should().BeFalse();
        facts.DaysUntilExpiry.Should().BeNull();
        facts.IsExpired.Should().BeFalse();
    }

    /// <summary>
    /// The reason the boolean exists: a null countdown means two different
    /// things, and this is the one where nothing has ended.
    /// </summary>
    [Fact]
    public void ALiveRegistrationWithNoExpiryDateIsStillOnTheTimeline()
    {
        var facts = ExpiryVisibility.For(
            RegistrationStatus.Approved, null, Today);

        facts.HasRunningValidity.Should().BeTrue();
        facts.DaysUntilExpiry.Should().BeNull();
        facts.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void ARegistrationNotYetGrantedIsStillOnTheTimeline()
    {
        ExpiryVisibility
            .For(RegistrationStatus.Planned, null, Today)
            .HasRunningValidity.Should().BeTrue();
    }

    /// <summary>
    /// Whether the countdown runs is decided by the same lifecycle table every
    /// transition answers to, not by a second list of statuses kept beside it.
    /// </summary>
    [Fact]
    public void RunningValidityFollowsTheLifecycleRatherThanItsOwnList()
    {
        foreach (var status in Enum.GetValues<RegistrationStatus>())
        {
            ExpiryVisibility
                .For(status, new DateOnly(2030, 1, 1), Today)
                .HasRunningValidity
                .Should().Be(
                    !RegistrationLifecycle.IsTerminal(status),
                    "{0} should follow the lifecycle's own answer", status);
        }
    }
}
