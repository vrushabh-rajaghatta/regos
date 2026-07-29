namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// Governance lifecycle of a template's identity. A retired template is
/// <see cref="Deprecated"/>, never deleted, so submissions that were built
/// against it remain explainable.
/// </summary>
public enum RegulatoryTemplateStatus
{
    Active = 1,
    Deprecated = 2,
}
