using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.ProductDocument.Application.Commands.UploadDocumentVersion;

/// <param name="Content">
/// The revised file. The caller owns the stream; the handler buffers it once so
/// the bytes it hashes and the bytes it stores are the same bytes.
/// </param>
public sealed record UploadDocumentVersionCommand(
    ProductDocumentId ProductDocumentId,
    string OriginalFileName,
    string ContentType,
    Stream Content);
