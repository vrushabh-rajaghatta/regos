namespace RegOS.Labeling.Domain.Aggregates.DrugInteractions;

public static class DrugInteractionErrors
{
    public const string TypeRequired =
        "The kind of interaction is required.";

    public const string TypeNotRecognised =
        "That kind of interaction is not recognised.";

    public const string SeverityNotRecognised =
        "That severity is not recognised.";

    public static readonly string LabelTextTooLong =
        $"Label text must be {DrugInteraction.LabelTextMaxLength} characters or fewer.";

    public static readonly string ManagementTooLong =
        $"Management advice must be {DrugInteraction.ManagementMaxLength} characters or fewer.";

    public const string InteractantRequired =
        "Name what this product interacts with.";

    public static readonly string InteractantTooLong =
        $"An interactant must be {Interactant.DescriptionMaxLength} characters or fewer.";

    public const string InteractantNotFound =
        "That interactant does not belong to this interaction.";

    /// <summary>
    /// The invariant S005 added to the context. An interaction with nothing to
    /// interact with is not an under-specified statement; it is not one.
    /// </summary>
    public const string LastInteractantCannotBeRemoved =
        "An interaction must name at least one thing it is with. Add another "
        + "before removing this one.";
}
