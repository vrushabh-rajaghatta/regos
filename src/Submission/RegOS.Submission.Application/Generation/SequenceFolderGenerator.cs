using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.SharedKernel.Exceptions;
using RegOS.Storage;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Generation;

/// <summary>
/// Writes one published sequence to disk — the folder, the files, and the
/// checksums. <b>No XML.</b>
/// </summary>
/// <remarks>
/// <b>EPIC-007a S004, and the first RegOS code ever to produce part of an eCTD
/// package.</b> It proves one thing and refuses four:
/// <list type="bullet">
/// <item>the same published submission generates <b>byte-identical</b> output
/// every time — which is ADR-049's *"the package is a projection"* stated as
/// something that can fail.</item>
/// <item>a draft, a paper filing, a sequence predating the activity model, and
/// one whose vocabulary or placement has never been read are each refused, in
/// their own words.</item>
/// </list>
/// <para>
/// <b>Nothing is written until everything is checked.</b> A refusal after the
/// first file is on disk leaves a half-built directory that looks like a
/// package, which is worse than no directory at all.
/// </para>
/// </remarks>
public sealed class SequenceFolderGenerator
{
    private readonly RegOSDbContext _dbContext;
    private readonly IFileStorage _storage;

    public SequenceFolderGenerator(
        RegOSDbContext dbContext, IFileStorage storage)
    {
        _dbContext = dbContext;
        _storage = storage;
    }

    /// <param name="destinationRoot">
    /// Where the <c>0000</c> folder is created. <b>Supplied by the caller and
    /// owned by the caller</b> — not a configured location, because a
    /// configured location would quietly become storage, and ADR-049 says v1
    /// stores nothing.
    /// </param>
    public async Task<GeneratedSequenceFolder> GenerateAsync(
        SubmissionId submissionId,
        string destinationRoot,
        CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.Submissions
            .AsNoTracking()
            .Include(s => s.Documents)
            .SingleOrDefaultAsync(s => s.Id == submissionId, cancellationToken)
            ?? throw new NotFoundException(
                SequenceGenerationErrors.SubmissionDoesNotExist);

        var plan = await PlanAsync(submission, cancellationToken);

        return await WriteAsync(plan, destinationRoot, cancellationToken);
    }

    /// <summary>
    /// Everything that can be refused, refused — and everything that will be
    /// written, resolved — before a single byte reaches the disk.
    /// </summary>
    private async Task<SequencePlan> PlanAsync(
        SubmissionAggregate submission, CancellationToken cancellationToken)
    {
        if (submission.Status != SubmissionStatus.Published
            || submission.SequenceNumber is not { } sequenceNumber)
        {
            throw new BusinessRuleViolationException(
                SequenceGenerationErrors.OnlyAFiledSequenceHasAPackage);
        }

        // The proof ADR-047 §4 asked for: the derivation is format-independent,
        // the rendering is not, and paper stops here.
        if (submission.Format != SubmissionFormat.Ectd)
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.OnlyEctdSequencesAreRendered,
                submission.Format));
        }

        // Refusal 1 — our history. Unrecoverable (evidence E13).
        if (!submission.IsClassified)
        {
            throw new BusinessRuleViolationException(
                SequenceGenerationErrors.SequencePredatesTheActivityModel);
        }

        await RequireWireVocabularyAsync(submission, cancellationToken);

        if (submission.BoundTemplateVersionId is not { } versionId)
        {
            throw new BusinessRuleViolationException(
                SequenceGenerationErrors.SubmissionIsUnbound);
        }

        var folders = await ResolveSectionFoldersAsync(versionId, cancellationToken);
        var leaves = await ResolveLeavesAsync(submission, folders, cancellationToken);

        return new SequencePlan(sequenceNumber, leaves);
    }

    /// <summary>
    /// Refusal 2 — the authority's vocabulary. Checked here rather than in the
    /// renderer that consumes it, so the failure arrives before the folder
    /// exists.
    /// </summary>
    /// <remarks>
    /// <b>This story writes no XML and still checks the tokens</b>, and that is
    /// deliberate. The rule it states needs no renderer's knowledge — <i>every
    /// reference-data value this sequence points at that carries a token must
    /// have one</i> — and a package half-built before S006 discovers a missing
    /// <c>fdast</c> is a package nobody can act on.
    /// </remarks>
    private async Task RequireWireVocabularyAsync(
        SubmissionAggregate submission, CancellationToken cancellationToken)
    {
        var applicationType = await (
            from application in _dbContext.RegulatoryApplications.AsNoTracking()
            where application.Id == submission.ApplicationId
            join type in _dbContext.ApplicationTypes
                on application.ApplicationTypeId equals type.Id
            select new { type.Code, type.Token }).SingleAsync(cancellationToken);

        RequireToken("Application type", applicationType.Code, applicationType.Token);

        // The activity's type lives on the sequence that opened it — this one,
        // or the one it continues (S003's exclusive-or, read from the far side).
        var typeOwner = submission.OriginatingSubmissionId ?? submission.Id;

        var submissionType = await (
            from sequence in _dbContext.Submissions.AsNoTracking()
            where sequence.Id == typeOwner
            join type in _dbContext.SubmissionTypes
                on sequence.SubmissionTypeId equals type.Id
            select new { type.Code, type.Token }).SingleAsync(cancellationToken);

        RequireToken("Regulatory activity", submissionType.Code, submissionType.Token);

        var subType = await _dbContext.SubmissionSubTypes
            .AsNoTracking()
            .Where(x => x.Id == submission.SubmissionSubTypeId)
            .Select(x => new { x.Code, x.Token })
            .SingleAsync(cancellationToken);

        RequireToken("Sequence action", subType.Code, subType.Token);

        static void RequireToken(string kind, string code, string? token)
        {
            if (string.IsNullOrEmpty(token))
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.NoEctdTokenForClassification,
                    kind, code));
        }
    }

    /// <summary>
    /// Each section's path inside the sequence folder — its ancestors' folders
    /// joined, in order, skipping the levels that contribute nothing.
    /// </summary>
    /// <remarks>
    /// <b>An empty folder is a level that adds no directory; a null one has
    /// never been read.</b> Appendix 4 gives 2.7.1–2.7.6 a file row and no
    /// directory row — their documents belong in 2.7's folder — so collapsing
    /// the two would make two-thirds of Module 2 unbuildable.
    /// </remarks>
    private async Task<IReadOnlyDictionary<TemplateSectionId, string>>
        ResolveSectionFoldersAsync(
            RegulatoryTemplateVersionId versionId,
            CancellationToken cancellationToken)
    {
        var sections = await _dbContext.Set<TemplateSection>()
            .AsNoTracking()
            .Where(s => EF.Property<RegulatoryTemplateVersionId>(
                s, "RegulatoryTemplateVersionId") == versionId)
            .ToListAsync(cancellationToken);

        var byId = sections.ToDictionary(s => s.Id);
        var resolved = new Dictionary<TemplateSectionId, string>();

        foreach (var section in sections)
        {
            var segments = new List<string>();

            for (var node = section; node is not null;
                 node = node.ParentSectionId is { } p && byId.TryGetValue(p, out var parent)
                     ? parent : null)
            {
                if (node.EctdFolder is not { } folder)
                {
                    throw new BusinessRuleViolationException(string.Format(
                        SequenceGenerationErrors.NoEctdFolderForSection,
                        node.Code));
                }

                if (folder.Length > 0)
                    segments.Insert(0, folder);
            }

            resolved[section.Id] = string.Join('/', segments);
        }

        return resolved;
    }

    private async Task<IReadOnlyList<PlannedLeaf>> ResolveLeavesAsync(
        SubmissionAggregate submission,
        IReadOnlyDictionary<TemplateSectionId, string> folders,
        CancellationToken cancellationToken)
    {
        // An operation is a fact about a placement (ADR-045 §5): an attached
        // document that sits in no section produces no leaf and no file.
        var placed = submission.Documents
            .Where(d => d.TemplateSectionId is not null)
            .ToList();

        var versionIds = placed.Select(d => d.DocumentVersionId).ToList();

        var versions = await _dbContext.Set<DocumentVersion>()
            .AsNoTracking()
            .Where(v => versionIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var documentNames = await (
            from document in _dbContext.ProductDocuments.AsNoTracking()
            where placed.Select(p => p.ProductDocumentId).Contains(document.Id)
            select new { document.Id, document.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var planned = new List<PlannedLeaf>();

        // Ordered before anything is written, and ordered by what the package
        // contains rather than by what the database returned: two runs must
        // produce the same bytes in the same order (ADR-049).
        foreach (var document in placed
            .OrderBy(d => folders[d.TemplateSectionId!.Value], StringComparer.Ordinal)
            .ThenBy(d => documentNames[d.ProductDocumentId], StringComparer.Ordinal)
            .ThenBy(d => d.ProductDocumentId.Value))
        {
            var version = versions[document.DocumentVersionId];
            var folder = folders[document.TemplateSectionId!.Value];
            var fileName = FileNameFor(version.OriginalFileName);

            var relativePath = folder.Length == 0
                ? fileName
                : $"{folder}/{fileName}";

            if (planned.FirstOrDefault(p => p.RelativePath == relativePath)
                is { } clash)
            {
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.TwoDocumentsWouldShareAFileName,
                    documentNames[clash.ProductDocumentId],
                    documentNames[document.ProductDocumentId],
                    fileName));
            }

            planned.Add(new PlannedLeaf(
                relativePath,
                document.ProductDocumentId,
                document.DocumentVersionId,
                version.StoragePath));
        }

        return planned;
    }

    /// <summary>
    /// ICH Appendix 2 applied to a file name: lowercase, <c>a-z0-9-</c>, and the
    /// extension kept.
    /// </summary>
    /// <remarks>
    /// A pure function of the stored name, so the same document always produces
    /// the same file — which is half of why regeneration is byte-identical.
    /// </remarks>
    public static string FileNameFor(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).TrimStart('.');
        var stem = Path.GetFileNameWithoutExtension(originalFileName);

        var slug = Slug(stem);
        if (slug.Length == 0)
            slug = "document";

        return extension.Length == 0 ? slug : $"{slug}.{Slug(extension)}";
    }

    private static string Slug(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var c in value.ToLowerInvariant())
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
                builder.Append(c);
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        return builder.ToString().Trim('-');
    }

    private async Task<GeneratedSequenceFolder> WriteAsync(
        SequencePlan plan,
        string destinationRoot,
        CancellationToken cancellationToken)
    {
        // 0000, not 0001 — the number RegOS holds (evidence E4). Every FDA
        // example starts at 0001 (E5), and that comparison belongs to S008.
        var root = Path.Combine(
            destinationRoot, plan.SequenceNumber.ToString("0000"));

        Directory.CreateDirectory(root);

        var leaves = new List<GeneratedLeaf>(plan.Leaves.Count);

        foreach (var leaf in plan.Leaves)
        {
            var absolute = Path.Combine(
                root, leaf.RelativePath.Replace('/', Path.DirectorySeparatorChar));

            Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

            await using var source =
                await _storage.OpenReadAsync(leaf.StoragePath, cancellationToken);
            await using var buffer = new MemoryStream();
            await source.CopyToAsync(buffer, cancellationToken);

            var bytes = buffer.ToArray();
            await File.WriteAllBytesAsync(absolute, bytes, cancellationToken);

            leaves.Add(new GeneratedLeaf(
                leaf.RelativePath,
                leaf.ProductDocumentId,
                leaf.DocumentVersionId,
                Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant(),
                bytes.LongLength));
        }

        var utilities = await WriteDtdsAsync(root, cancellationToken);

        return new GeneratedSequenceFolder(root, leaves, utilities);
    }

    /// <summary>
    /// <c>util/dtd/</c> — the DTDs the package must carry (Appendix 4 #371–376).
    /// </summary>
    /// <remarks>
    /// Only the region being filed to needs its regional DTD, which #371 states
    /// outright, so one regional file rather than four.
    /// <para>
    /// <b>The file names are RegOS's, following a pattern the appendix
    /// states.</b> #371 says rows 372–379 are *"illustrative only … consult
    /// regional guidance for the current name and version"*, and the pattern is
    /// <c>ich-ectd-n.dtd</c> where <c>n</c> is the version. This writes
    /// <c>3-2</c> and <c>3-3</c> accordingly — the same class of choice ADR-052
    /// governs for directories, and the DOCTYPE that S005 and S006 emit has to
    /// agree with what is written here.
    /// </para>
    /// </remarks>
    private static async Task<IReadOnlyList<string>> WriteDtdsAsync(
        string root, CancellationToken cancellationToken)
    {
        var written = new List<string>();

        foreach (var name in new[] { "ich-ectd-3-2.dtd", "us-regional-3-3.dtd" })
        {
            var relative = $"util/dtd/{name}";
            var absolute = Path.Combine(
                root, relative.Replace('/', Path.DirectorySeparatorChar));

            Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

            await using var resource = typeof(SequenceFolderGenerator).Assembly
                .GetManifestResourceStream(
                    $"RegOS.Submission.Application.Generation.{name}")
                ?? throw new InvalidOperationException(
                    $"The eCTD DTD '{name}' is not embedded in this build.");

            await using var file = File.Create(absolute);
            await resource.CopyToAsync(file, cancellationToken);

            written.Add(relative);
        }

        return written;
    }

    private sealed record SequencePlan(
        int SequenceNumber, IReadOnlyList<PlannedLeaf> Leaves);

    private sealed record PlannedLeaf(
        string RelativePath,
        RegOS.ProductDocument.Domain.IDs.ProductDocumentId ProductDocumentId,
        RegOS.ProductDocument.Domain.IDs.DocumentVersionId DocumentVersionId,
        string StoragePath);
}
