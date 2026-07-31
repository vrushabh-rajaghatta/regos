namespace RegOS.Registration.Domain.Aggregates.Registration;

public static class RegistrationErrors
{
    public const string TenantRequired =
        "Tenant is required.";

    public const string MedicinalProductRequired =
        "Medicinal product is required.";

    public const string AuthorityRequired =
        "Authority is required.";

    public const string HolderOrganizationRequired =
        "Holder organization is required.";

    public const string OccurredOnRequired =
        "The date the status took effect is required.";

    // Approval
    public const string RegistrationNumberRequired =
        "A registration number is required to record an approval.";

    public const string RegistrationNumberTooLong =
        "The registration number is too long.";

    public const string ApprovalAlreadyRecorded =
        "This registration's grant has already been recorded.";

    public const string ExpiryBeforeApproval =
        "A registration cannot expire before it was approved.";

    public const string NoteTooLong =
        "The note is too long.";

    // Lifecycle
    public const string StatusNotRecognised =
        "That is not a registration status.";

    public const string OccurredOnBeforePreviousEntry =
        "A status cannot take effect before the one it replaces. "
        + "History is read in business time.";

    /// <summary>
    /// The first grant establishes the registration number and validity dates,
    /// so it must go through the operation that captures them. Returning to
    /// Approved from Suspended is a lift, and carries no new grant.
    /// </summary>
    public const string ApprovalMustBeRecordedAsAGrant =
        "A registration is first approved by recording the grant, which "
        + "establishes its number and dates.";

    public static string AlreadyInStatus(RegistrationStatus status)
        => $"This registration is already {status}.";

    public static string StatusIsTerminal(RegistrationStatus status)
        => $"A {status} registration has reached the end of its lifecycle "
            + "and cannot change status.";

    public static string TransitionNotPermitted(
        RegistrationStatus from,
        RegistrationStatus to)
        => $"A registration cannot go from {from} to {to}.";
}
