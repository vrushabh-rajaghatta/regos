namespace RegOS.Registration.Application.Queries.ListRegistrationMarkets;

/// <summary>
/// One country this tenant holds registrations in, and how many.
/// </summary>
/// <remarks>
/// <b>Navigation, not analytics.</b> The count exists so a market reads as
/// "Canada (12)" and a person can tell a busy market from a quiet one before
/// clicking — not as a metric. Breakdowns by status, trends and charts are
/// EPIC-011, and keeping this shape deliberately thin is what lets that arrive
/// without changing this contract.
/// </remarks>
public sealed record RegistrationMarket(
    Guid CountryId,
    string CountryName,
    string CountryCode,
    int RegistrationCount);
