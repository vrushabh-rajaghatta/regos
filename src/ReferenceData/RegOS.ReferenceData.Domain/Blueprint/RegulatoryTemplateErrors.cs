namespace RegOS.ReferenceData.Domain.Blueprint;

public static class RegulatoryTemplateErrors
{
    public const string CodeRequired =
        "Regulatory template code is required.";

    public const string NameRequired =
        "Regulatory template name is required.";

    public const string AuthorityRequired =
        "Regulatory template must reference an authority.";

    public const string SubmissionTypeRequired =
        "Regulatory template must reference a submission type.";

    public const string SourceRequired =
        "Regulatory template source (provenance) is required.";

    public const string DraftAlreadyExists =
        "A draft version already exists; publish or discard it before starting another.";

    public const string VersionNotFound =
        "The specified version does not belong to this template.";

    public const string VersionAlreadyPublished =
        "The version is already published.";
}
