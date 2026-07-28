namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// How a failed validation rule is treated: <see cref="Error"/> blocks,
/// <see cref="Warning"/> advises. Mirrors the pass/fail/warning severities of
/// regulatory validation criteria (e.g. eCTD).
/// </summary>
public enum ValidationSeverity
{
    Error = 1,
    Warning = 2,
}
