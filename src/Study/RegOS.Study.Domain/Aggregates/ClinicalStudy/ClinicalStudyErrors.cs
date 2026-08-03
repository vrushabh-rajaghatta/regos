namespace RegOS.Study.Domain.Aggregates.ClinicalStudy;

public static class ClinicalStudyErrors
{
    public const string TenantRequired =
        "Tenant is required.";

    /// <summary>
    /// Worded as the sponsor's, because it is: ICH calls it *"the internal
    /// alphanumeric code used by the sponsor to unambiguously identify this
    /// study"* (E29). RegOS records it; it does not issue it (ADR-056).
    /// </summary>
    public const string SponsorStudyIdentifierRequired =
        "A study needs the identifier the sponsor uses for it.";

    public const string SponsorStudyIdentifierTooLong =
        "The sponsor's study identifier is too long.";

    public const string TitleRequired =
        "A study title is required.";

    public const string TitleTooLong =
        "The study title is too long.";
}
