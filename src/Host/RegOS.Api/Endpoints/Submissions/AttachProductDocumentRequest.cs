namespace RegOS.Api.Endpoints.Submissions;

/// <param name="TemplateSectionId">
/// Optional. Where in the bound blueprint the document lands — omitted, the
/// document is attached but unplaced.
/// </param>
public sealed record AttachProductDocumentRequest(
    Guid ProductDocumentId,
    Guid? TemplateSectionId = null);
