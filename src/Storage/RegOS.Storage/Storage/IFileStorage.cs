namespace RegOS.Storage;

/// <summary>
/// Where bytes live. Expressed in business terms: the caller gives a relative
/// path — <c>products/{GlobalProductId}/{ProductDocumentId}/v1.pdf</c>, or
/// <c>correspondence/{HaCorrespondenceId}/{AttachmentId}</c> — and the
/// implementation combines it with a configured root, keeping stored paths
/// portable across environments.
/// </summary>
/// <remarks>
/// <b>Its own module, and not because two contexts happened to need it.</b> It
/// lived in <c>ProductDocument.Application</c> until EPIC-006 S002. Leaving it
/// there would have made <c>Interaction</c> depend on <c>ProductDocument</c>
/// for an infrastructure concern — a domain dependency created by nothing more
/// than which context needed files first.
/// <para>
/// Not <c>RegOS.SharedKernel</c> either: ADR-017 rule 1 admits <em>concepts</em>,
/// not patterns, and storage carries no domain meaning. This is how ADR-040
/// decision 5's constraint — <b>do not build a second document store</b> — is
/// honoured without fusing two domains because both happen to hold files.
/// </para>
/// </remarks>
public interface IFileStorage
{
    Task SaveAsync(
        string relativePath,
        Stream content,
        CancellationToken cancellationToken);

    /// <summary>
    /// Opens the stored bytes for reading. Added in EPIC-006 S002: the first
    /// user question the port could not answer was <em>"show me what they
    /// actually sent"</em>.
    /// </summary>
    Task<Stream> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken);
}
