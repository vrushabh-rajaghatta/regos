namespace RegOS.RegulatoryApplication.Application;

/// <summary>
/// Business-rule violation messages surfaced by the creation policy.
/// </summary>
public static class RegulatoryApplicationErrors
{
    public const string ProductDoesNotExist = "Product does not exist.";

    public const string CountryDoesNotExist = "Country does not exist.";

    public const string AuthorityDoesNotExist = "Authority does not exist.";

    public const string OrganizationDoesNotExist = "Organization does not exist.";

    public const string OrganizationInactive = "Organization is inactive.";

    public const string ApplicationTypeDoesNotExist =
        "Application Type does not exist.";

    public const string AuthorityNotInCountry =
        "Authority does not belong to the selected country.";

    public const string DuplicateApplication =
        "A Application already exists for this product, country and authority.";

    public const string ApplicationDoesNotExist = "Application does not exist.";

    // Study citations (EPIC-019 S004, cross-context: studies are their own)
    public const string StudyDoesNotExist = "Study does not exist.";

    public const string CiteOneStudyAtATime =
        "Cite one study at a time. Name either a clinical or a non-clinical "
            + "study, not both.";

    public const string NoStudyNamed =
        "Name the study this application is supported by.";
}
