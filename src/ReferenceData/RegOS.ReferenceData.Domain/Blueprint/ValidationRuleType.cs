namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// The closed set of blueprint validation checks. Deliberately a code enum, not
/// user-defined data: every new rule type arrives with its own code, tests,
/// migration and documentation — governance a regulated system needs. The
/// engine that executes these lives in a later epic; here they are pure data.
/// </summary>
public enum ValidationRuleType
{
    /// <summary>Restricts the accepted file format(s); parameters hold the
    /// allowed extension(s), e.g. "pdf" or "pdf,docx".</summary>
    FileFormat = 1,

    /// <summary>Asserts a section must contain at least one document; no
    /// parameters.</summary>
    SectionNotEmpty = 2,
}
