namespace RegOS.ReferenceData.Application.Queries.Organization.ListIdentifierSchemes;

/// <param name="Issuer">
/// Who issues it — "Dun &amp; Bradstreet", "US FDA". Shown beside the code,
/// because "DUNS" alone does not tell a new user what they are choosing.
/// </param>
public sealed record IdentifierSchemeDto(
    Guid Id,
    string Code,
    string Name,
    string Issuer);
