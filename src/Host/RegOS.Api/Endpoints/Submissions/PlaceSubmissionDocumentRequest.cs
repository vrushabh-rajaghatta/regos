namespace RegOS.Api.Endpoints.Submissions;

/// <param name="TemplateSectionId">
/// Where the document should sit. Null takes it out of the dossier structure
/// without detaching it.
/// </param>
public sealed record PlaceSubmissionDocumentRequest(Guid? TemplateSectionId);
