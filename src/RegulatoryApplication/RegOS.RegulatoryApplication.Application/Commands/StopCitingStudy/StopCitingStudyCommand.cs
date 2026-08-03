using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Commands.StopCitingStudy;

/// <param name="StudyId">
/// The study, as a plain guid: a citation is withdrawn by naming the study, and
/// at this point the kind adds nothing — the application cites it once or not at
/// all.
/// </param>
public sealed record StopCitingStudyCommand(
    RegulatoryApplicationId ApplicationId,
    Guid StudyId);
