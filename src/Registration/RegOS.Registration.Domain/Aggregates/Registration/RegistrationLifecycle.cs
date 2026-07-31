namespace RegOS.Registration.Domain.Aggregates.Registration;

/// <summary>
/// The legal evolution of a registration's state: which status may follow which.
/// </summary>
/// <remarks>
/// Declared as a table rather than scattered conditionals so the permitted graph
/// is readable in one place, exhaustively testable, and so future capabilities
/// (renewal, restoration) arrive as edits to a matrix rather than as new
/// branches spread through the aggregate.
/// <para>
/// The governing principle: <b>forbid transitions that make the record
/// incoherent; permit transitions that are merely unusual.</b> RegOS must not
/// encode one regulator's process as universal law.
/// </para>
/// <para>
/// <b>Forward jumps are permitted from every pre-decision state.</b> A migrated
/// authorisation granted in 2019 never passed through RegOS's
/// <see cref="RegistrationStatus.Submitted"/> or
/// <see cref="RegistrationStatus.UnderReview"/>, and recording it as approved is
/// not skipping steps — it is faithfully recording that RegOS entered the story
/// after those steps had already happened.
/// </para>
/// <para>
/// <b>Three states are terminal, for three different reasons</b> — see
/// <see cref="IsTerminal"/>.
/// </para>
/// </remarks>
public static class RegistrationLifecycle
{
    private static readonly IReadOnlyDictionary<
        RegistrationStatus,
        IReadOnlySet<RegistrationStatus>> Transitions =
        new Dictionary<RegistrationStatus, IReadOnlySet<RegistrationStatus>>
        {
            // Pre-decision: may move forward through the process, reach either
            // decision, or be withdrawn — a sponsor pulling a filing before the
            // authority decides is ordinary.
            [RegistrationStatus.Planned] = Set(
                RegistrationStatus.Submitted,
                RegistrationStatus.UnderReview,
                RegistrationStatus.Approved,
                RegistrationStatus.Refused,
                RegistrationStatus.Withdrawn),

            [RegistrationStatus.Submitted] = Set(
                RegistrationStatus.UnderReview,
                RegistrationStatus.Approved,
                RegistrationStatus.Refused,
                RegistrationStatus.Withdrawn),

            [RegistrationStatus.UnderReview] = Set(
                RegistrationStatus.Approved,
                RegistrationStatus.Refused,
                RegistrationStatus.Withdrawn),

            // Granted: may be suspended, lapse, or be surrendered.
            [RegistrationStatus.Approved] = Set(
                RegistrationStatus.Suspended,
                RegistrationStatus.Expired,
                RegistrationStatus.Withdrawn),

            // Suspension is a reversible operational state, not the destruction
            // of the authorisation: the grant still exists, it merely cannot be
            // exercised. Lifting it is the expected outcome, so refusing the
            // return to Approved would be the surprising choice.
            [RegistrationStatus.Suspended] = Set(
                RegistrationStatus.Approved,
                RegistrationStatus.Expired,
                RegistrationStatus.Withdrawn),

            [RegistrationStatus.Refused] = Set(),
            [RegistrationStatus.Expired] = Set(),
            [RegistrationStatus.Withdrawn] = Set(),
        };

    /// <summary>
    /// Whether a registration in <paramref name="from"/> may become
    /// <paramref name="to"/>. A status never permits itself: staying in the same
    /// state while something else changes is a different operation, not a
    /// transition.
    /// </summary>
    public static bool Permits(RegistrationStatus from, RegistrationStatus to)
        => From(from).Contains(to);

    /// <summary>
    /// Every status a registration currently in <paramref name="status"/> may
    /// become — empty when it is terminal. Exposed so a caller can offer exactly
    /// the choices the domain would accept, rather than restating the table.
    /// </summary>
    public static IReadOnlySet<RegistrationStatus> From(RegistrationStatus status)
        => Transitions.TryGetValue(status, out var permitted)
            ? permitted
            : Set();

    /// <summary>
    /// Whether the registration's story has ended. Three states are terminal,
    /// and they are <em>not</em> the same kind of terminal:
    /// <list type="bullet">
    /// <item><see cref="RegistrationStatus.Refused"/> — permanently. No
    /// authorisation ever existed, so there is nothing to suspend, expire or
    /// surrender. A later grant would be a different registration.</item>
    /// <item><see cref="RegistrationStatus.Expired"/> — until renewal is
    /// modelled. Returning to Approved without a new validity period would leave
    /// a stale <c>ExpiresOn</c>; renewal changes validity, not status, and is a
    /// distinct operation.</item>
    /// <item><see cref="RegistrationStatus.Withdrawn"/> — until restoration is
    /// modelled. Some regulators do permit a surrendered authorisation to be
    /// restored. This is a deliberate boundary of the current domain, <b>not</b>
    /// an assertion that all regulators prohibit it.</item>
    /// </list>
    /// </summary>
    public static bool IsTerminal(RegistrationStatus status)
        => From(status).Count == 0;

    private static IReadOnlySet<RegistrationStatus> Set(
        params RegistrationStatus[] statuses)
        => statuses.ToHashSet();
}
