namespace RegOS.ReferenceData.Application.Queries.DocumentTypes.ListDocumentTypes;

public sealed record DocumentTypeDto(
    Guid Id,
    string Code,
    string Name,
    string? Description);
