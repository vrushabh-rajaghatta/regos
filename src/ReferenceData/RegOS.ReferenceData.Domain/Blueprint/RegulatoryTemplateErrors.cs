namespace RegOS.ReferenceData.Domain.Blueprint;

public static class RegulatoryTemplateErrors
{
    public const string CodeRequired =
        "Regulatory template code is required.";

    public const string NameRequired =
        "Regulatory template name is required.";

    public const string AuthorityRequired =
        "Regulatory template must reference an authority.";

    public const string ApplicationTypeRequired =
        "Regulatory template must reference an application type.";

    public const string SourceRequired =
        "Regulatory template source (provenance) is required.";

    public const string DraftAlreadyExists =
        "A draft version already exists; publish or discard it before starting another.";

    public const string VersionNotFound =
        "The specified version does not belong to this template.";

    public const string VersionAlreadyPublished =
        "The version is already published.";

    public const string OnlyPublishedVersionsCanBeDeprecated =
        "Only a published version can be deprecated — a draft that should not "
        + "be used is discarded, not deprecated.";

    public const string VersionAlreadyDeprecated =
        "The version is already deprecated.";

    public const string SectionCodeRequired =
        "Template section code is required.";

    public const string SectionTitleRequired =
        "Template section title is required.";

    // ICH Appendix 2 — a folder name becomes a filename, so an illegal one is
    // a package a regulator's tooling rejects, not a cosmetic defect.
    public const string SectionEctdFolderNeedsSource =
        "An eCTD folder name must say where it came from, and a source without "
        + "a folder claims nothing.";

    public const string SectionEctdFolderSourceNotRecognised =
        "That is not a source RegOS recognises for an eCTD folder name.";

    public const string SectionEctdFolderNotLegal =
        "An eCTD folder name may contain only lowercase letters, digits and "
        + "hyphens, and each part must be 64 characters or fewer.";

    public const string NoDraftVersion =
        "There is no draft version to modify; start a draft first.";

    public const string VersionNotDraft =
        "A published version's structure is frozen and cannot be changed.";

    public const string DuplicateSectionCode =
        "A section with this code already exists in the version.";

    public const string ParentSectionNotFound =
        "The parent section does not belong to this version.";

    public const string RequiredDocumentTypeRequired =
        "A required document must reference a document type.";

    public const string RequiredDocumentSectionNotFound =
        "The section does not belong to this version.";

    public const string DuplicateRequiredDocument =
        "This document type is already required in the section.";

    public const string ValidationRuleCodeRequired =
        "Validation rule code is required.";

    public const string ValidationRuleMessageRequired =
        "Validation rule message is required.";

    public const string ValidationRuleSectionNotFound =
        "The section does not belong to this version.";

    public const string DuplicateValidationRuleCode =
        "A validation rule with this code already exists in the version.";
}
