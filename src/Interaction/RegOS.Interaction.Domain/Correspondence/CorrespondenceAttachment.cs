using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Domain.Correspondence;

/// <summary>
/// The persisted content of a letter — the PDF that arrived, or the one we
/// sent.
/// </summary>
/// <remarks>
/// <b>Not a document.</b> `ProductDocument` is a business concept: it has a CTD
/// type, an approval lifecycle and numbered versions, and people ask questions
/// about it directly. An attachment is none of those — it is simply the bytes
/// of the correspondence it hangs from, and no user question reaches one
/// without going through the letter first. That is why it is a child entity and
/// why there is deliberately no <c>CorrespondenceDocument</c> root: a symmetric
/// name would recreate exactly the abstraction ADR-040 decision 5 argued away.
/// <para>
/// <b>No version number, and no lifecycle.</b> An inbound letter arrives once
/// and our reply is sent once; a v2 of a letter someone else wrote is not a
/// thing, and nobody drafts, activates and archives a letter the FDA sent them.
/// Correcting an attachment means removing it and attaching the right one —
/// which leaves the correspondence, the business record, untouched.
/// </para>
/// <para>
/// <b>Our reply is not an attachment on the request.</b> It is its own
/// correspondence, <c>Direction = Outbound</c>, with its own content.
/// <em>Threading is a relationship between correspondence records, not between
/// correspondence and attachments</em> — recorded so nobody later "solves" it
/// by adding response documents here.
/// </para>
/// </remarks>
public sealed class CorrespondenceAttachment
{
    public const int FileNameMaxLength = 255;

    // Internal: only HaCorrespondence (same assembly) can create one. There is
    // no path for application code to make an attachment without its letter.
    internal CorrespondenceAttachment(
        CorrespondenceAttachmentId id,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string storagePath,
        DateTime uploadedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new DomainException(HaCorrespondenceErrors.FileNameRequired);

        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException(HaCorrespondenceErrors.ContentTypeRequired);

        if (string.IsNullOrWhiteSpace(storagePath))
            throw new DomainException(HaCorrespondenceErrors.StoragePathRequired);

        if (fileSizeBytes <= 0)
            throw new DomainException(HaCorrespondenceErrors.FileEmpty);

        var trimmedName = originalFileName.Trim();

        if (trimmedName.Length > FileNameMaxLength)
            throw new DomainException(HaCorrespondenceErrors.FileNameTooLong);

        Id = id;
        OriginalFileName = trimmedName;
        ContentType = contentType.Trim();
        FileSizeBytes = fileSizeBytes;
        StoragePath = storagePath.Trim();
        UploadedOnUtc = uploadedOnUtc;
    }

    public CorrespondenceAttachmentId Id { get; } = default!;

    /// <summary>What it was called when it arrived. Preserved on download.</summary>
    public string OriginalFileName { get; } = default!;

    public string ContentType { get; } = default!;

    public long FileSizeBytes { get; }

    public string StoragePath { get; } = default!;

    public DateTime UploadedOnUtc { get; }
}
