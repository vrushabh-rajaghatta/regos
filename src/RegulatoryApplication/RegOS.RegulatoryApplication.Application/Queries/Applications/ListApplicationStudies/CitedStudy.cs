namespace RegOS.RegulatoryApplication.Application.Queries.Applications.ListApplicationStudies;

/// <param name="Kind"><c>Clinical</c> or <c>NonClinical</c>.</param>
/// <param name="SponsorStudyIdentifier">
/// The sponsor's code — <b>"Study ID"</b> on screen, and what a user recognises.
/// </param>
public sealed record CitedStudy(
    Guid StudyId,
    string Kind,
    string SponsorStudyIdentifier,
    string Title,
    DateTime CitedOn);
