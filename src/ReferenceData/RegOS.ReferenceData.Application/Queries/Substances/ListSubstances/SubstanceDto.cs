namespace RegOS.ReferenceData.Application.Queries.Substances.ListSubstances;

/// <param name="IsShared">
/// True when the platform ships it, false when this tenant added it. Surfaced
/// because the two behave differently — one is a governed fact nobody here can
/// change, the other is the organisation's own — and a directory that hid the
/// difference would leave a user guessing why one row offers no edit.
/// </param>
public sealed record SubstanceDto(
    Guid Id,
    string Name,
    string? Inn,
    CodedConceptDto SubstanceClass,
    CodedConceptDto SubstanceType,
    string? CasNumber,
    string? UniiCode,
    string? MolecularFormula,
    string? Description,
    bool IsShared);
