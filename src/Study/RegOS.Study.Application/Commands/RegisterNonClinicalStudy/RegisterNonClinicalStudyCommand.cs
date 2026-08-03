namespace RegOS.Study.Application.Commands.RegisterNonClinicalStudy;

/// <param name="SponsorStudyIdentifier">
/// The sponsor's own code for the study — not RegOS's id and not the
/// authority's. Screen word: <b>Study ID</b>.
/// </param>
/// <param name="Title">
/// The full title of the study, not the title of any one document in it.
/// </param>
public sealed record RegisterNonClinicalStudyCommand(
    string SponsorStudyIdentifier,
    string Title);
