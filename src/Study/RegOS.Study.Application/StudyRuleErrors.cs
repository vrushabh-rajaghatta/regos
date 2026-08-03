namespace RegOS.Study.Application;

/// <summary>
/// Rules that span the two study aggregates, so they cannot live in either.
/// </summary>
public static class StudyRuleErrors
{
    /// <summary>
    /// Names the identifier because that is what the user has to change, and
    /// says which study already holds it so they can tell a duplicate from a
    /// typo.
    /// </summary>
    public static string SponsorStudyIdentifierAlreadyUsed(
        string identifier,
        string byTitle)
        => $"Study \"{identifier}\" already exists — {byTitle}. "
            + "A sponsor study identifier names one study, because that is how "
            + "the authority recognises it across sequences.";

    public const string StudyDoesNotExist =
        "Study does not exist.";
}
