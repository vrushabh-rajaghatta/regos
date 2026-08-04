namespace RegOS.ReferenceData.Application.Queries.Substances.ListSubstances;

/// <summary>
/// Which half of the directory to show.
/// </summary>
/// <remarks>
/// A filter and not a permission: the query filter already decides what this
/// tenant may see (ADR-031). This only narrows it, which is why
/// <see cref="Any"/> is the default.
/// </remarks>
public enum SubstanceOrigin
{
    Any = 0,

    /// <summary>The catalogue the platform ships.</summary>
    Shared = 1,

    /// <summary>Compounds this tenant added.</summary>
    Proprietary = 2
}
