namespace RegOS.Labeling.Domain.Aggregates.GlobalLabels;

/// <summary>
/// Where one version of a global label sits in its life.
/// </summary>
/// <remarks>
/// <b>The act is <em>publish</em>; the state is <see cref="InForce"/>.</b> Two
/// different words on purpose — a regulatory user asks which version is in force
/// on a date, not which version was published, and those diverge the moment a
/// version is approved in March to take effect in June.
/// </remarks>
public enum GlobalLabelVersionStatus
{
    /// <summary>Being written. The only state in which anything can change.</summary>
    Draft = 0,

    /// <summary>Published and current. At most one version is ever here.</summary>
    InForce = 1,

    /// <summary>
    /// Was in force, and a later version replaced it. Still readable — a label
    /// that was in force when a submission cited it must stay quotable forever.
    /// </summary>
    Superseded = 2
}
