using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Submission.Application.Generation;

/// <summary>
/// What was written to disk for one published sequence.
/// </summary>
/// <remarks>
/// <b>Returned, never stored</b> (ADR-049). Deleting the folder loses no
/// business information — every fact in it came from the published submission,
/// which is frozen — so this is a description of a projection, not a record of
/// one.
/// </remarks>
/// <param name="RootPath">
/// The sequence folder itself: <c>…/0000</c>. Named for the sequence number
/// RegOS holds, which is <c>0000</c> for a first filing (evidence E4) even
/// though every FDA example begins at <c>0001</c> (E5). The business fact wins;
/// the convention is compared, not adopted, at S008.
/// </param>
public sealed record GeneratedSequenceFolder(
    string RootPath,
    IReadOnlyList<GeneratedLeaf> Leaves,
    IReadOnlyList<string> UtilityFiles);

/// <summary>
/// One document, written where the blueprint says it belongs.
/// </summary>
/// <param name="RelativePath">
/// Where it sits inside the sequence folder — the ancestor chain of
/// <c>TemplateSection.EctdFolder</c> values, then the file.
/// </param>
/// <param name="Md5">
/// <b>Computed here, not reused.</b> <c>DocumentVersion.Checksum</c> is SHA-256
/// and answers <i>"has this document changed?"</i>; eCTD requires MD5 and
/// answers <i>"did this file arrive intact?"</i>. Two questions, two
/// algorithms — and the stored one cannot be substituted for the wire one.
/// </param>
public sealed record GeneratedLeaf(
    string RelativePath,
    ProductDocumentId ProductDocumentId,
    DocumentVersionId DocumentVersionId,
    string Md5,
    long Bytes);
