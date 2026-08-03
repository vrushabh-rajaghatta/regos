using System.Security.Cryptography;
using System.Text;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Storage;
using RegOS.Submission.Application.Generation;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Domain.Submission;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// <b>EPIC-007a S004 — the sequence folder.</b>
///
/// The first RegOS code to produce part of an eCTD package, and the story's
/// headline acceptance criterion is not that files appear. It is that
/// <b>generating twice produces byte-identical output</b>, which is ADR-049's
/// *"the generated package is a projection, not a domain artifact"* written as
/// something that can fail.
/// </summary>
public sealed class SequenceFolderGeneratorTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private static readonly DocumentTypeId SeededCoverLetter =
        new(Guid.Parse("50000000-0000-0000-0000-000000000002"));

    private readonly List<Guid> _submissionIds = [];
    private readonly List<Guid> _documentIds = [];
    private readonly List<string> _outputRoots = [];

    // Tenant-aware: isolation is fail-closed (ADR-031), so a context without
    // one sees no organizations and the fixture cannot build an application.
    private static RegOSDbContext New() => new(
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString).Options,
        TestTenant.Context);

    /// <summary>
    /// Bytes in memory. The generator's job is where a file goes and what its
    /// checksum is — not how the document store keeps it — so the store is the
    /// one part of this that does not need to be real.
    /// </summary>
    private sealed class InMemoryStorage : IFileStorage
    {
        private readonly Dictionary<string, byte[]> _files = [];

        public void Put(string path, string content) =>
            _files[path] = Encoding.UTF8.GetBytes(content);

        public Task SaveAsync(string p, Stream c, CancellationToken _)
        {
            using var buffer = new MemoryStream();
            c.CopyTo(buffer);
            _files[p] = buffer.ToArray();
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string p, CancellationToken _) =>
            Task.FromResult<Stream>(new MemoryStream(_files[p]));

        public Task DeleteAsync(string p, CancellationToken _)
        {
            _files.Remove(p);
            return Task.CompletedTask;
        }
    }

    // --- The acceptance criterion --------------------------------------------

    /// <summary>
    /// <b>The projection thesis, stated as a falsifiable claim.</b> A published
    /// submission is frozen, so if regenerating it can differ, the package holds
    /// something the submission does not — and ADR-049 is wrong.
    /// </summary>
    [Fact]
    public async Task GeneratingTwice_ProducesByteIdenticalOutput()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var first = await GenerateAsync(ctx, storage, submissionId);
        var second = await GenerateAsync(ctx, storage, submissionId);

        Fingerprint(first.RootPath).Should().Equal(Fingerprint(second.RootPath));
    }

    [Fact]
    public async Task EveryPlacedDocument_IsWrittenWhereItsBlueprintSaysItBelongs()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var generated = await GenerateAsync(ctx, storage, submissionId);

        // 1.2 Cover Letters sits under Module 1's own regional directory:
        // m1/us from ICH Appendix 4, 12-cover-letters from ADR-052.
        generated.Leaves.Should().ContainSingle()
            .Which.RelativePath.Should().Be(
                "m1/us/12-cover-letters/cover-letter.pdf");

        File.Exists(Path.Combine(
            generated.RootPath, "m1", "us", "12-cover-letters",
            "cover-letter.pdf")).Should().BeTrue();
    }

    /// <summary>
    /// The folder is named for the number RegOS holds. Every FDA example starts
    /// at 0001 (E5) and 0000 is legal (E4); the business fact wins, and the
    /// divergence is compared at S008 rather than assumed away here.
    /// </summary>
    [Fact]
    public async Task TheSequenceFolder_IsNamedForTheNumberThatWasFiled()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var generated = await GenerateAsync(ctx, storage, submissionId);

        Path.GetFileName(generated.RootPath).Should().Be("0000");
    }

    /// <summary>
    /// eCTD requires MD5. <c>DocumentVersion.Checksum</c> is SHA-256 and answers
    /// a different question, so it cannot be reused however convenient that
    /// would be.
    /// </summary>
    [Fact]
    public async Task EachLeafCarriesAnMd5_OfTheBytesActuallyWritten()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var generated = await GenerateAsync(ctx, storage, submissionId);
        var leaf = generated.Leaves.Single();

        var onDisk = await File.ReadAllBytesAsync(
            Path.Combine(generated.RootPath,
                leaf.RelativePath.Replace('/', Path.DirectorySeparatorChar)));

        leaf.Md5.Should().Be(
            Convert.ToHexString(MD5.HashData(onDisk)).ToLowerInvariant());
        leaf.Md5.Should().HaveLength(32);
    }

    /// <summary>
    /// Appendix 4 #371-376: every package carries its DTDs, and only the region
    /// being filed to needs its regional one.
    /// </summary>
    [Fact]
    public async Task ThePackageCarriesItsOwnDtds()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var generated = await GenerateAsync(ctx, storage, submissionId);

        generated.UtilityFiles.Should().BeEquivalentTo(
            ["util/dtd/ich-ectd-3-2.dtd", "util/dtd/us-regional-3-3.dtd"]);

        var ich = await File.ReadAllTextAsync(Path.Combine(
            generated.RootPath, "util", "dtd", "ich-ectd-3-2.dtd"));

        // The embedded file is the one docs/evidence pins, not a copy of it —
        // so the DTD a package ships is the DTD the Level 2a claim was checked
        // against.
        ich.Should().Contain("<!ELEMENT ectd:ectd");
    }

    // --- The refusals ---------------------------------------------------------

    [Fact]
    public async Task ADraft_HasNoPackage()
    {
        await using var ctx = New();
        var (submissionId, storage) =
            await APublishedEctdSequenceAsync(ctx, publish: false);

        var act = async () => await GenerateAsync(ctx, storage, submissionId);

        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .WithMessage(SequenceGenerationErrors.OnlyAFiledSequenceHasAPackage);
    }

    /// <summary>
    /// ADR-047 §4 asserted the derivation is format-independent; this is the
    /// proof the <em>rendering</em> is not.
    /// </summary>
    [Fact]
    public async Task APaperFiling_NeverReachesTheRenderer()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(
            ctx, format: SubmissionFormat.Paper);

        var act = async () => await GenerateAsync(ctx, storage, submissionId);

        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .WithMessage("*not as eCTD*");
    }

    /// <summary>
    /// <b>The two refusals must not converge.</b> A sequence filed before the
    /// activity model has a gap in <em>our</em> history that nobody can close by
    /// reading a specification — unlike a missing wire token, which is exactly
    /// that. Same failure to generate, two different things to do about it.
    /// </summary>
    [Fact]
    public async Task ASequencePredatingTheActivityModel_SaysSo_AndNotSomethingElse()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        // Drop the classification the way history did: with no way to recover it.
        await ctx.Database.ExecuteSqlRawAsync(
            """
            UPDATE "Submissions"
            SET "SubmissionSubTypeId" = NULL, "SubmissionTypeId" = NULL,
                "OriginatingSubmissionId" = NULL
            WHERE "Id" = {0}
            """, submissionId.Value);

        var act = async () => await GenerateAsync(ctx, storage, submissionId);

        var thrown = await act.Should()
            .ThrowAsync<BusinessRuleViolationException>();

        thrown.WithMessage(
            SequenceGenerationErrors.SequencePredatesTheActivityModel);

        // The distinction, asserted rather than assumed: this is about our
        // records, not about an unread specification.
        thrown.Which.Message.Should().NotContain("eCTD token");
    }

    // --- File naming ----------------------------------------------------------

    /// <summary>
    /// ICH Appendix 2 applied to a file name, and a pure function of the stored
    /// one — which is half of why regeneration is byte-identical.
    /// </summary>
    [Theory]
    [InlineData("Cover Letter.pdf", "cover-letter.pdf")]
    [InlineData("IND_1571 (signed).PDF", "ind-1571-signed.pdf")]
    [InlineData("  spaced  out .pdf", "spaced-out.pdf")]
    [InlineData("no-extension", "no-extension")]
    [InlineData("....pdf", "document.pdf")]
    public void AFileName_IsNormalisedToWhatAppendix2Allows(
        string original, string expected)
    {
        SequenceFolderGenerator.FileNameFor(original).Should().Be(expected);
    }

    // --- Fixtures -------------------------------------------------------------

    private async Task<GeneratedSequenceFolder> GenerateAsync(
        RegOSDbContext ctx, IFileStorage storage, SubmissionId submissionId)
    {
        var root = Path.Combine(
            Path.GetTempPath(), "regos-s004", Guid.NewGuid().ToString("N"));

        _outputRoots.Add(root);

        return await new SequenceFolderGenerator(ctx, storage)
            .GenerateAsync(submissionId, root);
    }

    /// <summary>Every file under a root, with its bytes hashed — order included.</summary>
    private static List<string> Fingerprint(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => (
                Relative: Path.GetRelativePath(root, path)
                    .Replace(Path.DirectorySeparatorChar, '/'),
                Hash: Convert.ToHexString(
                    MD5.HashData(File.ReadAllBytes(path)))))
            .OrderBy(x => x.Relative, StringComparer.Ordinal)
            .Select(x => $"{x.Relative} {x.Hash}")
            .ToList();

    private async Task<(SubmissionId, InMemoryStorage)> APublishedEctdSequenceAsync(
        RegOSDbContext ctx,
        bool publish = true,
        SubmissionFormat format = SubmissionFormat.Ectd)
    {
        var (applicationId, globalProductId) =
            await TestFdaApplication.EnsureAsync(ctx);

        var storage = new InMemoryStorage();

        var document = ProductDocumentAggregate.Create(
            TestTenant.Id, globalProductId, SeededCoverLetter,
            $"Cover Letter {Guid.NewGuid()}");

        var storagePath =
            $"products/{globalProductId.Value}/{document.Id.Value}/v1.pdf";

        document.AddInitialVersion(
            originalFileName: "Cover Letter.pdf",
            storedFileName: "v1.pdf",
            contentType: "application/pdf",
            fileSize: 14,
            storagePath: storagePath,
            // SHA-256 is what the store keeps; the package needs MD5, and the
            // generator computes its own rather than reusing this.
            checksum: "sha256-x");
        document.Activate();

        storage.Put(storagePath, "a cover letter");

        ctx.ProductDocuments.Add(document);
        await ctx.SaveChangesAsync();
        _documentIds.Add(document.Id.Value);

        var coverLetterSection = await CoverLettersSectionAsync(ctx);

        var submission = SubmissionAggregate.Create(
            TestTenant.Id, applicationId, $"S004 {Guid.NewGuid()}", format,
            TestSubmissionClassification.Opens(),
            await BoundVersionAsync(ctx));

        submission.AttachDocument(
            document.Id, document.CurrentVersionId!.Value, coverLetterSection);

        if (publish)
            submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        ctx.Submissions.Add(submission);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(submission.Id.Value);

        return (submission.Id, storage);
    }

    private static async Task<RegOS.ReferenceData.Domain.Blueprint.RegulatoryTemplateVersionId>
        BoundVersionAsync(RegOSDbContext ctx) =>
        (await ctx.RegulatoryTemplates.AsNoTracking()
            .Include(t => t.Versions)
            .SelectMany(t => t.Versions)
            .Where(v => v.Status ==
                RegOS.ReferenceData.Domain.Blueprint.TemplateVersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstAsync(v => v.Sections.Any(s => s.Code == "1.2"))).Id;

    private static async Task<RegOS.ReferenceData.Domain.Blueprint.TemplateSectionId>
        CoverLettersSectionAsync(RegOSDbContext ctx)
    {
        var versionId = await BoundVersionAsync(ctx);

        return (await ctx.RegulatoryTemplates.AsNoTracking()
            .Include(t => t.Versions).ThenInclude(v => v.Sections)
            .SelectMany(t => t.Versions)
            .Where(v => v.Id == versionId)
            .SelectMany(v => v.Sections)
            .FirstAsync(s => s.Code == "1.2")).Id;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        await ctx.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Submissions\" WHERE \"Id\" = ANY({0})",
            _submissionIds.ToArray());
        await ctx.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"ProductDocuments\" WHERE \"Id\" = ANY({0})",
            _documentIds.ToArray());

        foreach (var root in _outputRoots.Where(Directory.Exists))
            Directory.Delete(root, recursive: true);
    }
}
