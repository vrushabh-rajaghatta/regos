namespace RegOS.Process.Application.Queries.GetProcessObjective;

/// <param name="MedicinalProductId">
/// The market record that fulfils this objective, once one exists. Null is the
/// normal state of a proposed objective (ADR-065 D8).
/// <para>
/// <b>Its name is deliberately absent, and the invariant is why.</b> A market
/// record is identified by its product and country, and D8 requires those to be
/// the pair this objective already holds — so a name here would be a second copy
/// of <c>ProductName</c> and <c>CountryName</c>, which is exactly the drift the
/// invariant exists to prevent.
/// </para>
/// </param>
/// <param name="Rationale">
/// Why this, and why this route — the strategy content, and the reason an
/// objective is its own aggregate rather than a field on a plan.
/// </param>
public sealed record ProcessObjectiveDetails(
    Guid Id,
    string Name,
    string? Rationale,
    Guid GlobalProductId,
    string ProductName,
    string CountryCode,
    string CountryName,
    Guid? MedicinalProductId,
    Guid? RegulatoryApplicationId,
    Guid? OwnerUserId,
    string Status,
    DateOnly StatedOn,
    DateOnly? TargetCompletionOn,
    DateOnly? AchievedOn,
    IReadOnlyList<ProcessObjectiveHistoryEntry> History);
