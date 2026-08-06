namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

public static class ProcessDefinitionErrors
{
    public const string CodeRequired =
        "A playbook needs a code.";

    public const string CodeTooLong =
        "That playbook code is too long.";

    public const string NameRequired =
        "A playbook needs a name.";

    public const string NameTooLong =
        "That playbook name is too long.";

    public const string CountryRequired =
        "A playbook applies to a market. Choose a country.";

    public const string AuthorityRequired =
        "A playbook applies to an authority.";

    public const string ApplicationTypeRequired =
        "A playbook applies to a kind of application.";

    public const string VersionNotFound =
        "That version does not belong to this playbook.";

    public const string DraftAlreadyOpen =
        "This playbook already has an open draft. Publish it, or discard it, "
        + "before starting another.";

    public const string NoOpenDraft =
        "This playbook has no open draft.";

    // --- version lifecycle (ADR-065 I4) -------------------------------------

    public const string VersionNotDraft =
        "A published version is frozen. Start a new draft instead.";

    public const string VersionAlreadyPublished =
        "That version is already published.";

    public const string OnlyPublishedVersionsCanBeSuperseded =
        "Only a published version can be superseded — a draft nobody used is "
        + "discarded instead.";

    public const string VersionAlreadySuperseded =
        "That version is already superseded.";

    // --- steps ---------------------------------------------------------------

    public const string StepCodeRequired =
        "A step needs a code.";

    public const string StepCodeTooLong =
        "That step code is too long.";

    public const string StepNameRequired =
        "A step needs a name.";

    public const string StepNameTooLong =
        "That step name is too long.";

    public const string StepDescriptionTooLong =
        "That step description is too long.";

    public const string DuplicateStepCode =
        "This version already has a step with that code.";

    public const string StepNotFound =
        "That step does not belong to this version.";

    public const string ParentStepNotFound =
        "That parent step does not belong to this version.";

    public const string StepCannotBeItsOwnParent =
        "A step cannot be its own parent.";

    public const string PredecessorNotFound =
        "That predecessor does not belong to this version.";

    public const string StepCannotPrecedeItself =
        "A step cannot come after itself.";

    public const string DuplicatePredecessor =
        "That step is already a predecessor of this one.";

    public const string OffsetDaysNegative =
        "A step cannot start before the thing it waits for. Use zero for "
        + "\"as soon as the predecessor finishes\".";

    public const string DurationDaysNotPositive =
        "A step takes at least a day.";

    /// <summary>
    /// The rule <see cref="ProcessDefinition.PublishVersion"/> exists to enforce:
    /// a cycle makes the schedule underivable, and the playbook — not the plan —
    /// is the thing that is wrong (ADR-065 decision 4).
    /// </summary>
    public const string PredecessorCycle =
        "These steps wait for each other in a circle, so no start date can be "
        + "worked out. Break the loop before publishing.";

    public const string NoSteps =
        "A playbook with no steps has nothing to instantiate.";
}
