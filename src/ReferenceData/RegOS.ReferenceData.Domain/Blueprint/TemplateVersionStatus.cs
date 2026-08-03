namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// Lifecycle of a single template version. A <see cref="Draft"/> is editable;
/// once <see cref="Published"/> it is frozen and immutable; a
/// <see cref="Deprecated"/> version stays readable forever but binds nothing new.
/// </summary>
public enum TemplateVersionStatus
{
    Draft = 1,

    Published = 2,

    /// <summary>
    /// Published, then superseded. <b>Not deleted, and not editable.</b>
    /// </summary>
    /// <remarks>
    /// Submissions already bound to it keep their binding and keep working —
    /// they were judged against what the blueprint said at the time, and that
    /// is a fact about those filings (ADR-036, ES-018). What deprecation stops
    /// is <em>new</em> bindings: a version we know to be wrong must not govern
    /// work that has not started yet.
    /// <para>
    /// Moving an existing draft onto a newer version is a separate capability
    /// and a deliberate user action — changing the blueprint underneath
    /// someone's draft changes what their draft means.
    /// </para>
    /// </remarks>
    Deprecated = 3,
}
