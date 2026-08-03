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

        var placements =
            await ResolveSectionPlacementsAsync(versionId, cancellationToken);

        var priorSequences =
            await ResolvePriorSequencesAsync(submission, cancellationToken);

        var leaves = await ResolveLeavesAsync(
            submission, placements, priorSequences, cancellationToken);

        var deletions = await ResolveDeletionsAsync(
            submission, placements, priorSequences, cancellationToken);

        // Checked here rather than while resolving, because the limit is on the
        // whole path and the sequence folder is part of it.
        foreach (var leaf in leaves)
            RequireAPathTheRegionAccepts(sequenceNumber, leaf.RelativePath);

        return new SequencePlan(sequenceNumber, leaves, deletions);
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
    private async Task<IReadOnlyDictionary<TemplateSectionId, SectionPlacement>>
        ResolveSectionPlacementsAsync(
            RegulatoryTemplateVersionId versionId,
            CancellationToken cancellationToken)
    {
        var sections = await _dbContext.Set<TemplateSection>()
            .AsNoTracking()
            .Where(s => EF.Property<RegulatoryTemplateVersionId>(
                s, "RegulatoryTemplateVersionId") == versionId)
            .ToListAsync(cancellationToken);

        var byId = sections.ToDictionary(s => s.Id);
        var resolved = new Dictionary<TemplateSectionId, SectionPlacement>();

        foreach (var section in sections)
        {
            var segments = new List<string>();
            var elements = new List<string>();

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

                if (node.IchElement is not { } element)
                {
                    throw new BusinessRuleViolationException(string.Format(
                        SequenceGenerationErrors.NoEctdElementForSection,
                        node.Code));
                }

                if (folder.Length > 0)
                    segments.Insert(0, folder);

                // The same chaining the folder column already does, for the
                // same reason: RegOS's tree is coarser than the CTD's in two
                // places, so one section carries two levels of backbone —
                // m3-2-body-of-data/m3-2-s-drug-substance. An empty value is a
                // section ICH gives no element at all, which is every Module 1
                // sub-section.
                if (element.Length > 0)
                {
                    elements.InsertRange(
                        0, element.Split('/', StringSplitOptions.RemoveEmptyEntries));
                }
            }

            resolved[section.Id] = new SectionPlacement(
                section.Code, string.Join('/', segments), elements);
        }

        return resolved;
    }

    /// <summary>
    /// The sequence folder each superseded or withdrawn placement was filed in
    /// — the <c>../0000/</c> half of a <c>modified-file</c> pointer.
    /// </summary>
    /// <remarks>
    /// A leaf ID is unique within its own sequence and is never reused across
    /// them (ICH Appendix 6), so the pointer needs both halves: which sequence,
    /// and which leaf inside it.
    /// </remarks>
    private async Task<IReadOnlyDictionary<SubmissionDocumentId, int>>
        ResolvePriorSequencesAsync(
            SubmissionAggregate submission,
            CancellationToken cancellationToken)
    {
        var referenced = submission.Documents
            .Select(d => d.ReplacesSubmissionDocumentId)
            .Concat(submission.Deletions
                .Select(d => (SubmissionDocumentId?)d.DeletesSubmissionDocumentId))
            .OfType<SubmissionDocumentId>()
            .Distinct()
            .ToList();

        if (referenced.Count == 0)
            return new Dictionary<SubmissionDocumentId, int>();

        var rows = await (
            from sequence in _dbContext.Submissions.AsNoTracking()
            from document in sequence.Documents
            where referenced.Contains(document.Id)
                && sequence.SequenceNumber != null
            select new { document.Id, sequence.SequenceNumber })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.Id, x => x.SequenceNumber!.Value);
    }

    /// <summary>
    /// A withdrawal — the one operation with no document behind it.
    /// </summary>
    /// <remarks>
    /// It produces a leaf and <b>no file</b>: ICH Appendix 6 Table 6-3 says
    /// *"there is no new file submitted in this case… the checksum attribute
    /// value will be empty"*. So the two things a leaf normally carries are both
    /// the empty string, and that is the specification's own instruction rather
    /// than a convenience.
    /// </remarks>
    private async Task<IReadOnlyList<BackboneLeaf>> ResolveDeletionsAsync(
        SubmissionAggregate submission,
        IReadOnlyDictionary<TemplateSectionId, SectionPlacement> placements,
        IReadOnlyDictionary<SubmissionDocumentId, int> priorSequences,
        CancellationToken cancellationToken)
    {
        if (submission.Deletions.Count == 0)
            return [];

        var names = await NamesOfAsync(
            submission.Deletions.Select(d => d.ProductDocumentId),
            cancellationToken);

        foreach (var deletion in submission.Deletions)
            RequireAWritableBackbonePosition(placements[deletion.TemplateSectionId]);

        return submission.Deletions
            .Select(deletion => new BackboneLeaf(
                placements[deletion.TemplateSectionId].IchElements,
                LeafId(deletion.DeletesSubmissionDocumentId),
                names[deletion.ProductDocumentId],
                Href: string.Empty,
                Operation: "delete",
                Checksum: string.Empty,
                ModifiedFile: ModifiedFile(
                    deletion.DeletesSubmissionDocumentId, priorSequences)))
            .OrderBy(x => string.Join('/', x.ElementPath), StringComparer.Ordinal)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToList();
    }

    private async Task<Dictionary<
        RegOS.ProductDocument.Domain.IDs.ProductDocumentId, string>> NamesOfAsync(
        IEnumerable<RegOS.ProductDocument.Domain.IDs.ProductDocumentId> ids,
        CancellationToken cancellationToken)
    {
        var wanted = ids.Distinct().ToList();

        return await _dbContext.ProductDocuments.AsNoTracking()
            .Where(document => wanted.Contains(document.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    /// <summary>
    /// An XML <c>ID</c> may not begin with a digit and a GUID often does, so the
    /// stored id is emitted with a letter in front of it and nothing else
    /// changed — a leaf stays traceable to its placement.
    /// </summary>
    private static string LeafId(SubmissionDocumentId id) =>
        $"leaf-{id.Value:D}";

    private static string? ModifiedFile(
        SubmissionDocumentId? replaces,
        IReadOnlyDictionary<SubmissionDocumentId, int> priorSequences)
    {
        if (replaces is not { } target
            || !priorSequences.TryGetValue(target, out var sequence))
        {
            return null;
        }

        return $"../{sequence:0000}/{IchBackboneRenderer.FileName}"
            + $"#{LeafId(target)}";
    }

    private async Task<IReadOnlyList<PlannedLeaf>> ResolveLeavesAsync(
        SubmissionAggregate submission,
        IReadOnlyDictionary<TemplateSectionId, SectionPlacement> placements,
        IReadOnlyDictionary<SubmissionDocumentId, int> priorSequences,
        CancellationToken cancellationToken)
    {
        // An operation is a fact about a placement (ADR-045 §5): an attached
        // document that sits in no section produces no leaf and no file.
        //
        // Unchanged is dropped here, and dropping it is the whole thesis. A
        // RegOS sequence holds the entire dossier; an eCTD sequence holds only
        // what changed, and there is no "unchanged" operation to emit. So a
        // carried-forward document produces no leaf — and, because a file
        // nothing references is a file a validator asks about, no file either.
        var placed = submission.Documents
            .Where(d => d.TemplateSectionId is not null
                && d.Operation is not null
                && d.Operation != SubmissionContentOperation.Unchanged)
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
            .OrderBy(d => placements[d.TemplateSectionId!.Value].Folder,
                StringComparer.Ordinal)
            .ThenBy(d => documentNames[d.ProductDocumentId], StringComparer.Ordinal)
            .ThenBy(d => d.ProductDocumentId.Value))
        {
            var version = versions[document.DocumentVersionId];
            var placement = placements[document.TemplateSectionId!.Value];

            RequireAWritableBackbonePosition(placement);

            var folder = placement.Folder;
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
                version.StoragePath,
                placement.IchElements,
                LeafId(document.Id),
                documentNames[document.ProductDocumentId],
                WireOperation(document.Operation!.Value),
                ModifiedFile(
                    document.ReplacesSubmissionDocumentId, priorSequences)));
        }

        return planned;
    }

    /// <summary>
    /// FDA accepts no path longer than 150 characters (evidence E22).
    /// </summary>
    /// <remarks>
    /// <b>The stricter of two published limits.</b> ICH Appendix 2 allows 230,
    /// so a path this refuses may be perfectly legal elsewhere — which is why the
    /// message names the region rather than the format.
    /// <para>
    /// <b>What is measured is what RegOS emits</b>: the sequence folder and
    /// everything under it, <c>0000/m3/32-body-data/…</c>. The application folder
    /// above it (<c>NDA123456/</c>) is the caller's, chosen at delivery, and it
    /// spends from the same 150 — so a package that passes here can still exceed
    /// the limit once it is placed. Measuring what we do not choose would be
    /// guessing; leaving the rest unsaid would be worse.
    /// </para>
    /// </remarks>
    private static void RequireAPathTheRegionAccepts(
        int sequenceNumber, string relativePath)
    {
        var path = $"{sequenceNumber:0000}/{relativePath}";

        if (path.Length > MaxPathLength)
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.PathTooLongForTheRegion,
                path, path.Length, MaxPathLength));
        }
    }

    /// <summary>FDA eCTD Technical Conformance Guide §2.4.</summary>
    private const int MaxPathLength = 150;

    /// <summary>
    /// Refusal 3 — the specification asks for a fact RegOS does not hold.
    /// </summary>
    /// <remarks>
    /// Checked while planning, so it lands before any file exists, and checked
    /// per <em>placed</em> section rather than per seeded one: a blueprint that
    /// merely offers 3.2.S is fine, and it is putting a document there that
    /// cannot be written down.
    /// </remarks>
    private static void RequireAWritableBackbonePosition(
        SectionPlacement placement)
    {
        foreach (var element in placement.IchElements)
        {
            if (IchBackboneRenderer.KeyedElements.TryGetValue(element, out var key))
            {
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.SectionNeedsAFactRegOsDoesNotHold,
                    placement.Code, element, key));
            }
        }
    }

    /// <summary>
    /// <c>operation (new | append | replace | delete)</c> — the DTD's own
    /// enumeration, closed and exhaustive (evidence E14).
    /// </summary>
    /// <remarks>
    /// <b>Read, never recomputed</b> (ADR-045). The value was decided at publish
    /// against the dossier as it then stood; deriving it again here would let a
    /// later rule change quietly rewrite what a filed sequence said.
    /// </remarks>
    private static string WireOperation(SubmissionContentOperation operation)
        => operation switch
        {
            SubmissionContentOperation.New => "new",
            SubmissionContentOperation.Replace => "replace",
            SubmissionContentOperation.Append => "append",

            // Unchanged never reaches here — it produces no leaf. Delete has no
            // SubmissionDocument behind it and arrives as a SubmissionDeletion.
            _ => throw new InvalidOperationException(
                $"'{operation}' does not describe a leaf in a filed sequence."),
        };

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

        var backbones = await WriteIchBackboneAsync(
            root, plan, leaves, cancellationToken);

        // Every emitted path, not only the leaves. These are fixed strings and
        // the check cannot fire today — which is the point: if a DTD is renamed
        // or a backbone moves, the limit is still enforced rather than assumed.
        foreach (var path in utilities.Concat(backbones))
            RequireAPathTheRegionAccepts(plan.SequenceNumber, path);

        return new GeneratedSequenceFolder(root, leaves, utilities, backbones);
    }

    /// <summary>
    /// <c>index.xml</c> and <c>index-md5.txt</c>, written last because the
    /// backbone quotes the checksum of every file beneath it.
    /// </summary>
    /// <remarks>
    /// <b>Module 1 is held back for S006.</b> ICH's
    /// <c>m1-administrative-information-and-prescribing-information</c> is
    /// declared <c>(leaf*)</c> with no children of its own — ICH defers the
    /// whole module to the regions — so a Module 1 document has no ICH element
    /// to sit under. Its leaves belong to the regional backbone, and the one
    /// leaf ICH's m1 does carry points at that file, which does not exist yet.
    /// </remarks>
    private static async Task<IReadOnlyList<string>> WriteIchBackboneAsync(
        string root,
        SequencePlan plan,
        IReadOnlyList<GeneratedLeaf> written,
        CancellationToken cancellationToken)
    {
        var checksums = written.ToDictionary(x => x.RelativePath, x => x.Md5);

        var leaves = plan.Leaves
            .Where(leaf => leaf.ElementPath.Count > 0
                && leaf.ElementPath[0] != ModuleOneElement)
            .Select(leaf => new BackboneLeaf(
                leaf.ElementPath,
                leaf.LeafId,
                leaf.Title,
                leaf.RelativePath,
                leaf.Operation,
                checksums[leaf.RelativePath],
                leaf.ModifiedFile))
            .Concat(plan.Deletions
                .Where(leaf => leaf.ElementPath.Count > 0
                    && leaf.ElementPath[0] != ModuleOneElement))
            .ToList();

        var xml = IchBackboneRenderer.Render(leaves);

        // UTF-8 without a BOM. The declaration says UTF-8 and a BOM would be a
        // second, silent claim about the same thing.
        var bytes = new UTF8Encoding(false).GetBytes(xml);

        await File.WriteAllBytesAsync(
            Path.Combine(root, IchBackboneRenderer.FileName),
            bytes,
            cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(root, IchBackboneRenderer.ChecksumFileName),
            Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant(),
            cancellationToken);

        return [IchBackboneRenderer.FileName, IchBackboneRenderer.ChecksumFileName];
    }

    /// <summary>
    /// ICH's Module 1 element. Named here because index.xml is the file that has
    /// to leave it alone, not because this renderer knows anything about a
    /// region.
    /// </summary>
    private const string ModuleOneElement =
        "m1-administrative-information-and-prescribing-information";

    /// <summary>
    /// <c>util/dtd/</c> — the DTDs the package must carry (Appendix 4 #371–376).
    /// </summary>
    /// <remarks>
    /// Only the region being filed to needs its regional DTD, which #371 states
    /// outright, so one regional file rather than four.
    /// <para>
    /// <b>The names are the published ones, not Appendix 4's pattern.</b> #371
    /// says rows 372–379 are *"illustrative only … consult regional guidance for
    /// the current name and version"* — so the pattern <c>us-regional-3-3.dtd</c>
    /// suggests is exactly what the appendix disclaims, and FDA publishes
    /// <c>us-regional-v3-3.dtd</c>. This is not ADR-052's territory: that
    /// governs names <em>nobody</em> published, and this one is published.
    /// </para>
    /// <para>
    /// The DOCTYPE each backbone emits, the embedded resource, and the file on
    /// disk all name the same string. A package that validates against a DTD it
    /// does not carry is the failure a reviewer cannot see and a regulator can.
    /// </para>
    /// </remarks>
    private static async Task<IReadOnlyList<string>> WriteDtdsAsync(
        string root, CancellationToken cancellationToken)
    {
        var written = new List<string>();

        foreach (var name in new[] { "ich-ectd-3-2.dtd", "us-regional-v3-3.dtd" })
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
        int SequenceNumber,
        IReadOnlyList<PlannedLeaf> Leaves,
        IReadOnlyList<BackboneLeaf> Deletions);

    private sealed record PlannedLeaf(
        string RelativePath,
        RegOS.ProductDocument.Domain.IDs.ProductDocumentId ProductDocumentId,
        RegOS.ProductDocument.Domain.IDs.DocumentVersionId DocumentVersionId,
        string StoragePath,
        IReadOnlyList<string> ElementPath,
        string LeafId,
        string Title,
        string Operation,
        string? ModifiedFile);

    /// <summary>
    /// Where a section's documents go — on disk, and in the backbone. Two
    /// answers to *"where does this belong?"*, resolved together because they
    /// walk the same ancestor chain.
    /// </summary>
    private sealed record SectionPlacement(
        string Code, string Folder, IReadOnlyList<string> IchElements);
}
