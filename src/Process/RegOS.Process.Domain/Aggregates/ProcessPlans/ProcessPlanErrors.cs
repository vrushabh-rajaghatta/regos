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
