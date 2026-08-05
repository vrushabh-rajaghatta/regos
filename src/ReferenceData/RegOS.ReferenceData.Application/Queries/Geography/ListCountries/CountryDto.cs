namespace RegOS.ReferenceData.Application.Queries.Geography.ListCountries;

/// <param name="Code">
/// ISO 3166-1 alpha-2 — what a picker shows beside the name.
/// </param>
/// <param name="IsoAlpha3Code">
/// ISO 3166-1 alpha-3 — what a machine-readable submission names the country
/// by, and <b>not derivable</b> from <paramref name="Code"/>.
/// </param>
/// <param name="IsoName">
/// The register's own wording — <em>"United Kingdom of Great Britain and
/// Northern Ireland"</em>. Sent so a caller that must quote it does not have to
/// guess; a screen keeps showing <paramref name="Name"/>.
/// </param>
public sealed record CountryDto(
    Guid Id,
    string Code,
    string IsoAlpha3Code,
    string Name,
    string IsoName);
