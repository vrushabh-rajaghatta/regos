namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

public static class ProcessPlanErrors
{
    public const string TenantRequired = "A plan belongs to a tenant.";

    public const string ObjectiveRequired =
        "A plan is how we achieve something. State the objective first.";

    public const string NameRequired = "A plan needs a name.";

    public const string NameTooLong = "That plan name is too long.";

    public const string NoteTooLong = "That note is too long.";

    /// <summary>
    /// I4, enforced at the consuming end: a draft is still being written and a
    /// superseded version is no longer instantiated from.
    /// </summary>
    public const string VersionNotPublished =
        "Only a published version of a playbook can be used to create a plan.";

    public const string AlreadyActive = "This plan is already active.";

    public const string HistoryOutOfOrder =
        "That date is before something already recorded on this plan.";

    public const string AlreadyClosed =
        "This plan is completed or cancelled. Create a new plan rather than "
        + "reopening it.";

    public const string NotActive =
        "Work can only be recorded against an active plan.";

    // --- steps ---------------------------------------------------------------

    public const string StepNotFound =
        "That step does not belong to this plan.";

    public const string StepAlreadySettled =
        "That step is already complete or skipped. Record a correction rather "
        + "than reopening it.";

    public const string StepAlreadyInProgress =
        "That step is already in progress.";

    public const string StepHistoryOutOfOrder =
        "That date is before something already recorded on this step.";

    /// <summary>
    /// The one place S004 adds friction on purpose. Six months later somebody
    /// asks why a step was not performed, and <em>"Skipped"</em> alone is not an
    /// answer.
    /// </summary>
    public const string EndBeforeStart =
        "A step cannot end before it starts.";

    public const string SkipReasonRequired =
        "Say why this step is being skipped. It becomes the record of why the "
        + "work was not done.";

    /// <summary>
    /// <b>Not a user error.</b> Publication certified that the definition is a
    /// valid DAG suitable for instantiation, so a graph that cannot be scheduled
    /// means RegOS broke one of its own guarantees between publish and here.
    /// Raised as <see cref="InvalidOperationException"/>, never a domain
    /// exception — the user could not have caused it and cannot fix it.
    /// </summary>
    public const string CertificateBroken =
        "The playbook version could not be scheduled, which publication should "
        + "have made impossible. Steps left unscheduled: ";
}
