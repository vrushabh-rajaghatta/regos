using RegOS.Platform.Contracts;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Process.Application.Commands.CreateProcessObjective;

/// <summary>
/// States an intention. <b>It carries no status</b> — a new objective is always
/// <c>Proposed</c>, and there is deliberately no parameter that could skip that
/// first state (ADR-065 decision 3: an objective is stated before it is taken up).
/// </summary>
public sealed record CreateProcessObjectiveCommand(
    GlobalProductId GlobalProductId,
    CountryId CountryId,
    string Name,
    DateOnly StatedOn,
    string? Rationale = null,
    UserId? OwnerUserId = null,
    DateOnly? TargetCompletionOn = null);
