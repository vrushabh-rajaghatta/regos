namespace RegOS.Registration.Domain.Aggregates.Registration;

/// <summary>
/// Where a market authorisation stands.
/// </summary>
/// <remarks>
/// A closed enum in code rather than governed reference data, deliberately:
/// reference data answers <em>what exists</em>, and this answers <em>what may
/// happen</em> — what can be renewed, what a variation may be filed against.
/// That is behaviour, and it belongs beside the behaviour.
/// <para>
/// Which of these may follow which — and which are terminal, and why — is
/// declared in <see cref="RegistrationLifecycle"/>.
/// </para>
/// </remarks>
public enum RegistrationStatus
{
    /// <summary>Intended, not yet filed.</summary>
    Planned = 1,

    /// <summary>Filed with the authority.</summary>
    Submitted = 2,

    /// <summary>The authority is assessing it.</summary>
    UnderReview = 3,

    /// <summary>Granted — the product may be marketed.</summary>
    Approved = 4,

    /// <summary>Granted, but temporarily not in force.</summary>
    Suspended = 5,

    /// <summary>Surrendered by the holder.</summary>
    Withdrawn = 6,

    /// <summary>Lapsed on its expiry date without renewal.</summary>
    Expired = 7,

    /// <summary>The authority refused to grant it.</summary>
    Refused = 8,
}
