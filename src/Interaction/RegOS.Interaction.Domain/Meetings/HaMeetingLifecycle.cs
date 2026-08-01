namespace RegOS.Interaction.Domain.Meetings;

/// <summary>
/// Which meeting statuses may follow which.
/// </summary>
/// <remarks>
/// <b>This exists to record that a meeting's lifecycle is qualitatively
/// different from the other four in this context — not to be reused.</b> Its
/// value is documentation, not code sharing; twenty lines that say "an outside
/// party decides this" are worth more than none.
/// <para>
/// The single reason it is here: <c>Requested → Granted | Declined</c> is a
/// branch <b>the authority chooses</b>. `HaQuestion`, `Commitment` and
/// `Inspection` progress through our own operational states, where a table
/// would be one company's habits mistaken for a rule.
/// </para>
/// <para>
/// Three terminal states, and each means something different: declined (they
/// said no), cancelled (it was called off), held (it happened). None of them
/// leads anywhere, because a second meeting is a second meeting.
/// </para>
/// </remarks>
internal static class HaMeetingLifecycle
{
    private static readonly Dictionary<HaMeetingStatus, HaMeetingStatus[]> Allowed = new()
    {
        [HaMeetingStatus.Requested] =
        [
            HaMeetingStatus.Granted,
            HaMeetingStatus.Declined,
            HaMeetingStatus.Cancelled
        ],
        [HaMeetingStatus.Granted] =
        [
            HaMeetingStatus.Held,
            HaMeetingStatus.Cancelled
        ],
        [HaMeetingStatus.Declined] = [],
        [HaMeetingStatus.Held] = [],
        [HaMeetingStatus.Cancelled] = []
    };

    public static bool Permits(HaMeetingStatus from, HaMeetingStatus to)
        => Allowed.TryGetValue(from, out var next) && next.Contains(to);

    public static bool IsTerminal(HaMeetingStatus status)
        => Allowed[status].Length == 0;
}
