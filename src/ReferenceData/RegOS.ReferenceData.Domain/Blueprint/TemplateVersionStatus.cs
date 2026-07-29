namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// Lifecycle of a single template version. A <see cref="Draft"/> is editable;
/// once <see cref="Published"/> it is frozen and immutable.
/// </summary>
public enum TemplateVersionStatus
{
    Draft = 1,
    Published = 2,
}
