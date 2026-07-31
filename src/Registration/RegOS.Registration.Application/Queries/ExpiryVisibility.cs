using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Queries;

/// <summary>
/// What a registration's validity period says today.
/// </summary>
/// <param name="HasRunningValidity">
/// Whether the registration is still on the validity timeline at all. False
/// once its lifecycle has ended — a surrendered authorisation is not counting
/// down towards anything.
/// <para>
/// Exists so a null <paramref name="DaysUntilExpiry"/> is self-explaining:
/// true means no expiry date was ever recorded, false means the countdown has
/// stopped mattering.
/// </para>
/// </param>
/// <param name="DaysUntilExpiry">
/// Days from today until the authorisation lapses. Null when there is no expiry
/// date, or when the registration is no longer on the timeline.
/// <para>
/// <b>Negative values are kept.</b> An approved registration whose expiry passed
/// last month reports -31, and that is the strongest attention signal the system
/// has: lapsed in the world, not yet recorded here. Clamping it to zero would
/// discard exactly the information worth surfacing.
/// </para>
/// </param>
/// <param name="IsExpired">
/// Whether that date has passed. Derivable from
/// <paramref name="DaysUntilExpiry"/>, and exposed anyway so no client has to
/// re-implement the sign convention.
/// </param>
public readonly record struct ExpiryFacts(
    bool HasRunningValidity,
    int? DaysUntilExpiry,
    bool IsExpired);

/// <summary>
/// Expiry proximity, derived on every read and never stored.
/// </summary>
/// <remarks>
/// Facts only. There is deliberately no <c>IsExpiringSoon</c>: "soon" is policy
/// — ninety days today, a hundred and eighty tomorrow, market-specific after
/// that, tenant-configurable eventually. <see cref="ExpiryFacts.DaysUntilExpiry"/>
/// never goes out of date; a threshold would.
/// <para>
/// Persist regulatory facts; derive regulatory interpretation. A stored
/// "expiring soon" flag would be wrong the moment the clock moved, and would
/// need a job to keep it honest.
/// </para>
/// </remarks>
public static class ExpiryVisibility
{
    public static ExpiryFacts For(
        RegistrationStatus status,
        DateOnly? expiresOn,
        DateOnly today)
    {
        // Whether the countdown is running is a lifecycle question, and it is
        // answered by the same table every transition answers to — not by a
        // second list of statuses kept here.
        var running = !RegistrationLifecycle.IsTerminal(status);

        if (!running || expiresOn is not { } expiry)
            return new ExpiryFacts(running, null, false);

        var days = expiry.DayNumber - today.DayNumber;

        return new ExpiryFacts(true, days, days < 0);
    }

    /// <summary>
    /// The read-side clock. Expiry is relative to the day it is asked about, so
    /// nothing about it can be persisted and stay true.
    /// </summary>
    public static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);
}
