namespace RegOS.ReferenceData.Application.Commands.CreateSubstance;

/// <summary>
/// Add a compound the shared catalogue does not carry.
/// </summary>
/// <remarks>
/// <b>No tenant on the command, and that is the point.</b> The handler takes it
/// from <c>ITenantContext</c>, so this capability cannot express "create a
/// shared substance" — the invariant is the absence of a parameter rather than
/// a check on one (ADR-058 §2).
/// </remarks>
/// <param name="SubstanceClassCode">
/// A code from <c>SubstanceVocabulary.Classes</c>, not a display name — the
/// wire carries the code so a re-worded label does not break a caller.
/// </param>
public sealed record CreateSubstanceCommand(
    string Name,
    string? Inn,
    string SubstanceClassCode,
    string SubstanceTypeCode,
    string? CasNumber = null,
    string? UniiCode = null,
    string? MolecularFormula = null,
    string? Description = null);
