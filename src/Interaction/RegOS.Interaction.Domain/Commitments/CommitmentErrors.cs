namespace RegOS.Interaction.Domain.Commitments;

public static class CommitmentErrors
{
    public const string TenantRequired = "A tenant is required.";

    public const string AuthorityRequired =
        "A commitment is made to an authority.";

    public const string TitleRequired = "A title is required.";

    public static readonly string TitleTooLong =
        $"A title cannot exceed {Commitment.TitleMaxLength} characters.";

    public static readonly string DescriptionTooLong =
        $"A description cannot exceed {Commitment.DescriptionMaxLength} characters.";

    public const string AlreadyInThatStatus =
        "The commitment already holds that status.";

    public const string AlreadyClosed =
        "A fulfilled or waived commitment cannot change status.";

    public const string CannotReopen =
        "A commitment cannot return to Open.";

    public const string HistoryOutOfOrder =
        "A commitment's history cannot go backwards in time.";

    public static readonly string NoteTooLong =
        $"A note cannot exceed {CommitmentStatusEntry.NoteMaxLength} characters.";
}
