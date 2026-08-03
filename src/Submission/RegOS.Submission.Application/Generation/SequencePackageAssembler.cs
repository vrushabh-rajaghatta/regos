using System.IO.Compression;

using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Generation;

/// <summary>
/// Assembles a published sequence into a single downloadable archive.
/// </summary>
/// <remarks>
/// <b>EPIC-007a S007 — delivery, and it deliberately creates nothing.</b> No
/// aggregate, no id, no status, no stored file (ADR-049): deleting the archive
/// loses no business information, because every fact in it came from a
/// submission that is frozen. It is a download, not a record.
/// <para>
/// <b>The temporary folder is the implementation's, not the caller's.</b>
/// <see cref="SequenceFolderGenerator"/> takes a destination precisely so that a
/// configured location cannot quietly become storage; this supplies one it
/// deletes, so the same rule holds one level up. A failed generation leaves
/// nothing behind either — the refusal happens before any byte is written, and
/// the folder is removed whether or not the archive was produced.
/// </para>
/// <para>
/// <b>Nothing here reads a validator.</b> The seam between RegOS and any
/// validator is the filesystem, and <c>ValidatorIndependenceTests</c> asserts
/// it — a package is assembled whether or not a parser is installed.
/// </para>
/// </remarks>
public sealed class SequencePackageAssembler
{
    private readonly SequenceFolderGenerator _generator;

    public SequencePackageAssembler(SequenceFolderGenerator generator)
    {
        _generator = generator;
    }

    /// <summary>
    /// Generates the sequence and returns it as a ZIP, in memory.
    /// </summary>
    /// <remarks>
    /// In memory because a package is a dossier's worth of PDFs for one
    /// sequence, not a dossier — and because streaming to a caller-held file
    /// would put the archive somewhere RegOS then has to reason about owning.
    /// <b>Revisit when a real filing is large enough to notice</b>, which is a
    /// measurement nobody can make yet.
    /// </remarks>
    public async Task<SequencePackage> AssembleAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken = default)
    {
        var scratch = Path.Combine(
            Path.GetTempPath(), "regos-ectd", Guid.NewGuid().ToString("N"));

        try
        {
            var folder = await _generator.GenerateAsync(
                submissionId, scratch, cancellationToken);

            using var buffer = new MemoryStream();

            // The sequence folder goes in at the archive root, so unpacking
            // produces 0000/ and nothing above it.
            //
            // RegOS does not invent the application folder. The mapping draws
            // one — "ctd-123456" — and marks it "e.g.": no specification this
            // repository holds names it, and it is the same across every
            // sequence of an application, which makes it a filing decision
            // rather than a property of this download.
            ZipFile.CreateFromDirectory(
                folder.RootPath,
                buffer,
                CompressionLevel.Optimal,
                includeBaseDirectory: true);

            return new SequencePackage(
                $"{Path.GetFileName(folder.RootPath)}.zip",
                buffer.ToArray(),
                folder);
        }
        finally
        {
            if (Directory.Exists(scratch))
                Directory.Delete(scratch, recursive: true);
        }
    }
}

/// <param name="FileName">
/// <c>0000.zip</c> — the sequence, named for the number RegOS holds.
/// </param>
/// <param name="Contents">
/// The archive itself. <b>Handed to the caller and kept nowhere</b>, which is
/// ADR-049 expressed as a signature rather than as a promise.
/// </param>
/// <param name="Folder">
/// What went into it, so a caller can say what was written without unpacking —
/// and so nothing has to be re-derived to describe the download.
/// </param>
public sealed record SequencePackage(
    string FileName,
    byte[] Contents,
    GeneratedSequenceFolder Folder);
