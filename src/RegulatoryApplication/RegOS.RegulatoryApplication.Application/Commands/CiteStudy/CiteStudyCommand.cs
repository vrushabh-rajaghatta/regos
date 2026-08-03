using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

namespace RegOS.RegulatoryApplication.Application.Commands.CiteStudy;

/// <summary>
/// Records that a study supports this application.
/// </summary>
/// <remarks>
/// Two typed properties rather than a <c>(kind, id)</c> pair, for the reason
/// ADR-056 §2 gives and <c>ReportStudyOnPlacementCommand</c> repeats: they name
/// two aggregates, and a discriminator here is where one in the domain would
/// start.
/// <para>
/// Unlike a placement's study, this is a **set** rather than a single value —
/// an application rests on many studies — so the command adds one rather than
/// stating the whole. Removal is its own command.
/// </para>
/// </remarks>
public sealed record CiteStudyCommand(
    RegulatoryApplicationId ApplicationId,
    ClinicalStudyId? ClinicalStudyId,
    NonClinicalStudyId? NonClinicalStudyId);
