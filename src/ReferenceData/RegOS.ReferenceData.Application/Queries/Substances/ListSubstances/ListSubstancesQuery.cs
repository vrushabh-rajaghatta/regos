namespace RegOS.ReferenceData.Application.Queries.Substances.ListSubstances;

/// <summary>
/// The substance directory: the shared catalogue and this tenant's own
/// compounds, in one list.
/// </summary>
/// <param name="Search">
/// Matched against name and INN. A user looking for a compound knows one of the
/// two and rarely which — searching only the preferred name would hide
/// acetylsalicylic acid from someone who typed it.
/// </param>
public sealed record ListSubstancesQuery(
    string? Search = null,
    SubstanceOrigin Origin = SubstanceOrigin.Any);
