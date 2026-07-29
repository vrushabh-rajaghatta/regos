using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// A checkable constraint a template version imposes — beyond structure
/// (sections) and content (required documents). Pure data: it names a check
/// (<see cref="RuleType"/>), how a failure is treated (<see cref="Severity"/>),
/// and an optional payload the executing engine (a later epic) interprets. A
/// child of the aggregate: created only through the template.
/// </summary>
public sealed class ValidationRule
{
    public const int CodeMaxLength = 100;
    public const int ParametersMaxLength = 500;
    public const int MessageMaxLength = 500;

    internal ValidationRule(
        ValidationRuleId id,
        string code,
        ValidationRuleType ruleType,
        ValidationSeverity severity,
        string message,
        TemplateSectionId? sectionId,
        string? parameters,
        int order)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(RegulatoryTemplateErrors.ValidationRuleCodeRequired);

        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException(RegulatoryTemplateErrors.ValidationRuleMessageRequired);

        Id = id;
        // Rule codes carry meaningful casing (e.g. "FDA-IND-M1-NONEMPTY") — trim only.
        Code = code.Trim();
        RuleType = ruleType;
        Severity = severity;
        Message = message.Trim();
        SectionId = sectionId;
        // Interpreted per RuleType; whitespace-only becomes null.
        Parameters = string.IsNullOrWhiteSpace(parameters) ? null : parameters.Trim();
        Order = order;
    }

    public ValidationRuleId Id { get; }

    public string Code { get; }

    public ValidationRuleType RuleType { get; }

    public ValidationSeverity Severity { get; }

    public string Message { get; }

    // null => the rule targets the whole version; otherwise a section in it.
    public TemplateSectionId? SectionId { get; }

    // Rule-type-specific payload (e.g. "pdf"); null when the type needs none.
    public string? Parameters { get; }

    public int Order { get; }
}
