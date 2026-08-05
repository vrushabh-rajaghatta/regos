using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Storage;
using RegOS.Submission.Application.Generation;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Domain.Submission;

using NonClinicalStudy =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

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
[Collection(SubmissionDatabase.Collection)]
public sealed class SequenceFolderGeneratorTests : IAsyncLifetime
{
    private readonly SubmissionDatabase _database;

    public SequenceFolderGeneratorTests(SubmissionDatabase database)
    {
        _database = database;
    }


    private static readonly DocumentTypeId SeededCoverLetter =
        new(Guid.Parse("50000000-0000-0000-0000-000000000002"));

    private readonly List<Guid> _submissionIds = [];
    private readonly List<Guid> _documentIds = [];
    private readonly List<string> _outputRoots = [];

    // Tenant-aware: isolation is fail-closed (ADR-031), so a context without
    // one sees no organizations and the fixture cannot build an application.
    private RegOSDbContext New() => new(
        _database.Options,
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

        // FDA's published name, not Appendix 4's illustrative pattern — #371
        // disclaims its own rows and defers to regional guidance.
        //
        // util/style/ arrived with EPIC-019: the STF stylesheet, and the
        // vocabulary it resolves by a relative path. Two files, because one of
        // them checks nothing without the other (E34).
        generated.UtilityFiles.Should().BeEquivalentTo([
            "util/dtd/ich-ectd-3-2.dtd",
            "util/dtd/us-regional-v3-3.dtd",
            "util/dtd/ich-stf-v2-2.dtd",
            "util/style/ich-stf-stylesheet-2-3.xsl",
            "util/style/valid-values.xml"
        ]);

        var ich = await File.ReadAllTextAsync(Path.Combine(
            generated.RootPath, "util", "dtd", "ich-ectd-3-2.dtd"));

        // The embedded file is the one docs/evidence pins, not a copy of it —
        // so the DTD a package ships is the DTD the Level 2a claim was checked
        // against.
        ich.Should().Contain("<!ELEMENT ectd:ectd");
    }

    // --- S005: the ICH backbone ----------------------------------------------

    /// <summary>
    /// <b>The story's outcome, end to end.</b> A real published sequence, its
    /// real blueprint placement, and the DTD the package itself carries — with
    /// the DOCTYPE resolved from the folder the generator built, so the path is
    /// checked as well as the content.
    /// </summary>
    [Fact]
    public async Task TheGeneratedPackage_CarriesAnIndexValidAgainstItsOwnDtd()
    {
        await using var ctx = New();
        var (submissionId, storage) =
            await APublishedEctdSequenceAsync(ctx, sectionCode: "2.3");

        var generated = await GenerateAsync(ctx, storage, submissionId);

        generated.BackboneFiles.Should().Contain("index.xml");

        var (exitCode, output) = ValidateIndex(generated.RootPath);
        exitCode.Should().Be(0, "xmllint said: {0}", output);

        var xml = await File.ReadAllTextAsync(
            Path.Combine(generated.RootPath, "index.xml"));

        // 2.3 sits under Module 2 in both trees: a directory on disk and an
        // element in the backbone, resolved from the same ancestor walk.
        xml.Should().Contain("<m2-common-technical-document-summaries>");
        xml.Should().Contain("<m2-3-quality-overall-summary>");
        xml.Should().Contain("operation=\"new\"");
        xml.Should().Contain("xlink:href=\"m2/23-qos/cover-letter.pdf\"");
    }

    /// <summary>Appendix 4 #2 — the backbone's checksum, beside the backbone.</summary>
    [Fact]
    public async Task TheIndexChecksum_IsTheChecksumOfTheIndex()
    {
        await using var ctx = New();
        var (submissionId, storage) =
            await APublishedEctdSequenceAsync(ctx, sectionCode: "2.3");

        var generated = await GenerateAsync(ctx, storage, submissionId);

        var index = await File.ReadAllBytesAsync(
            Path.Combine(generated.RootPath, "index.xml"));

        var recorded = await File.ReadAllTextAsync(
            Path.Combine(generated.RootPath, "index-md5.txt"));

        recorded.Should().Be(
            Convert.ToHexString(MD5.HashData(index)).ToLowerInvariant());
    }

    /// <summary>
    /// <b>The story boundary, and S006 moved it.</b> ICH declares its Module 1
    /// element <c>(leaf*)</c> and defers the module to the regions, so a Module
    /// 1 document is written to disk and named in the <em>regional</em>
    /// backbone. What <c>index.xml</c> carries is one leaf: a pointer at that
    /// file.
    /// </summary>
    /// <remarks>
    /// This test asserted the opposite until 2026-08-03 — <c>index.xml</c>
    /// mentioning Module 1 at all was the defect S005 was guarding against,
    /// because *"a backbone that links a missing file is worse than one that
    /// links nothing"*. The file now exists, so the link is written, and the
    /// assertion inverts with the behaviour rather than being deleted.
    /// </remarks>
    [Fact]
    public async Task AModuleOneDocument_IsNamedInTheRegionalBackbone_AndCrossLinkedFromTheIndex()
    {
        await using var ctx = New();

        // 1.2 Cover Letters — the default fixture, and Module 1 throughout.
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var generated = await GenerateAsync(ctx, storage, submissionId);

        generated.Leaves.Should().ContainSingle()
            .Which.RelativePath.Should()
            .Be("m1/us/12-cover-letters/cover-letter.pdf");

        var index = await File.ReadAllTextAsync(
            Path.Combine(generated.RootPath, "index.xml"));

        // The document itself is not here — only the regional file is.
        index.Should().NotContain("12-cover-letters");
        index.Should().Contain("xlink:href=\"m1/us/us-regional.xml\"");

        var regional = await File.ReadAllTextAsync(Path.Combine(
            generated.RootPath, "m1", "us", "us-regional.xml"));

        // And there the document is, one directory pair up from the backbone.
        regional.Should().Contain("m1-2-cover-letters");
        regional.Should().Contain(
            "xlink:href=\"../../m1/us/12-cover-letters/cover-letter.pdf\"");

        ValidateIndex(generated.RootPath).ExitCode.Should().Be(0);
    }

    /// <summary>
    /// <b>S006's acceptance criterion, on output RegOS generated.</b> The
    /// standing Level 2a evidence for <c>us-regional.xml</c> came from the
    /// renderer being handed a hand-built <c>RegionalBackbone</c>. This is the
    /// same DTD applied to a file built from a real published sequence — a real
    /// applicant's DUNS, a real contact, real placements.
    /// </summary>
    /// <remarks>
    /// The DOCTYPE is rewritten to the pinned DTD before validating, because
    /// what ships points at accessdata.fda.gov (E26) and this evidence is
    /// offline. Neither is bent to suit the other: the shipped header is
    /// asserted separately in FdaRegionalBackboneRendererTests.
    /// </remarks>
    [Fact]
    public async Task TheGeneratedPackage_CarriesARegionalBackboneValidAgainstItsOwnDtd()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var generated = await GenerateAsync(ctx, storage, submissionId);

        var path = Path.Combine(
            generated.RootPath, "m1", "us", "us-regional.xml");

        var offline = (await File.ReadAllTextAsync(path)).Replace(
            FdaRegionalBackboneRenderer.DoctypeSystemId,
            "../../util/dtd/us-regional-v3-3.dtd",
            StringComparison.Ordinal);

        await File.WriteAllTextAsync(path, offline, new UTF8Encoding(false));

        var result = Xmllint(path);

        result.ExitCode.Should().Be(0, result.Output);
    }

    /// <summary>
    /// <b>EPIC-007a's outcome sentence, and nothing before this asserted it.</b>
    /// Per-file validity is not package validity: S005 and S006 each check one
    /// file in isolation, and <b>E16 is the reason that is not enough</b> — the
    /// two backbones disagree about whether a leaf's checksum is required, so a
    /// package can fail as a whole while the file under test passes.
    /// </summary>
    /// <remarks>
    /// <b>One package, both files, and the link between them resolved.</b> The
    /// cross-link is the seam: <c>index.xml</c> names a file it does not
    /// contain, and no per-file check can tell whether that file is there.
    /// <para>
    /// This is also where the epic's Level 2a evidence is <b>re-earned</b>. The
    /// standing claim rests on <c>poc/ctd-987654/</c>, which was hand-written —
    /// it proves the target can be hit and says nothing about whether RegOS hits
    /// it.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task OnePackage_BothBackbonesValid_AndTheLinkBetweenThemResolves()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var generated = await GenerateAsync(ctx, storage, submissionId);

        // Every file the package claims to carry is on disk.
        foreach (var relative in generated.BackboneFiles
            .Concat(generated.UtilityFiles)
            .Concat(generated.Leaves.Select(x => x.RelativePath)))
        {
            File.Exists(Path.Combine(
                generated.RootPath,
                relative.Replace('/', Path.DirectorySeparatorChar)))
                .Should().BeTrue($"the package lists {relative}");
        }

        // The regional file validates offline (E26 — what ships points at
        // accessdata.fda.gov, and the evidence is a pinned DTD).
        var regionalPath = Path.Combine(
            generated.RootPath, "m1", "us", "us-regional.xml");

        await File.WriteAllTextAsync(
            regionalPath,
            (await File.ReadAllTextAsync(regionalPath)).Replace(
                FdaRegionalBackboneRenderer.DoctypeSystemId,
                "../../util/dtd/us-regional-v3-3.dtd",
                StringComparison.Ordinal),
            new UTF8Encoding(false));

        var index = Xmllint(
            Path.Combine(generated.RootPath, IchBackboneRenderer.FileName));
        var regional = Xmllint(regionalPath);

        index.ExitCode.Should().Be(0, index.Output);
        regional.ExitCode.Should().Be(0, regional.Output);

        // And the seam itself: index.xml points at the regional file by a path
        // relative to the sequence root, and that path resolves.
        var indexXml = await File.ReadAllTextAsync(
            Path.Combine(generated.RootPath, IchBackboneRenderer.FileName));

        var href = Regex.Match(indexXml, "xlink:href=\"([^\"]*us-regional[^\"]*)\"")
            .Groups[1].Value;

        href.Should().NotBeEmpty();

        File.Exists(Path.Combine(
            generated.RootPath, href.Replace('/', Path.DirectorySeparatorChar)))
            .Should().BeTrue("index.xml names a file the package must contain");
    }

    /// <summary>
    /// <b>The cross-link quotes a checksum of the file it points at</b>, which
    /// is why the regional backbone is written first. ICH makes a leaf's
    /// <c>checksum</c> <c>#REQUIRED</c> (E16), and an empty one here would be
    /// the E7 shape — a leaf with no file — which this is not.
    /// </summary>
    [Fact]
    public async Task TheCrossLink_CarriesTheRegionalBackbonesOwnChecksum()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var generated = await GenerateAsync(ctx, storage, submissionId);

        var regional = await File.ReadAllBytesAsync(Path.Combine(
            generated.RootPath, "m1", "us", "us-regional.xml"));

        var index = await File.ReadAllTextAsync(
            Path.Combine(generated.RootPath, "index.xml"));

        index.Should().Contain(
            Convert.ToHexString(MD5.HashData(regional)).ToLowerInvariant());
    }

    /// <summary>
    /// <b>The third refusal.</b> <c>m3-2-s-drug-substance</c> is a repeatable
    /// node the DTD will not accept without naming its substance and
    /// manufacturer; RegOS models 3.2.S as one section and records neither. That
    /// is not a gap in our history, nor an unread specification — the
    /// specification has been read and asks for a fact we do not hold.
    /// </summary>
    [Fact]
    public async Task ASectionKeyedByAFactWeDoNotHold_SaysWhichFact()
    {
        await using var ctx = New();
        var (submissionId, storage) =
            await APublishedEctdSequenceAsync(ctx, sectionCode: "3.2.S");

        var act = async () => await GenerateAsync(ctx, storage, submissionId);

        var thrown = await act.Should()
            .ThrowAsync<BusinessRuleViolationException>();

        thrown.Which.Message.Should().Contain("m3-2-s-drug-substance");
        thrown.Which.Message.Should().Contain("substance and manufacturer");

        // Not confused with either of the other two refusals.
        thrown.Which.Message.Should().NotContain("eCTD token");
        thrown.Which.Message.Should().NotContain("has not been read");
    }

    /// <summary>
    /// <b>The third refusal again, and the largest instance of it.</b> FDA
    /// requires a Study Tagging File for every file in 4.2.x (evidence E21); an
    /// STF names the study each document belongs to, and RegOS records no
    /// studies (ADR-054). The FDA IND blueprint seeds 4.2.1, 4.2.2 and 4.2.3 and
    /// every IND has nonclinical content, so this is a module rather than a
    /// section.
    /// </summary>
    /// <remarks>
    /// <b>No validator could have found this.</b> The leaf is perfectly valid
    /// XML without an STF — FDA's review tool simply files it under *"Not
    /// Applicable (N/A) or Unassigned Folders"* (eCTD TCG §4.3). Until this
    /// test, RegOS generated exactly that package.
    /// </remarks>
    [Fact]
    public async Task ADocumentInAStudyReportSection_IsRefused_UntilItNamesAStudy()
    {
        await using var ctx = New();
        var (submissionId, storage) =
            await APublishedEctdSequenceAsync(ctx, sectionCode: "4.2.3");

        var act = async () => await GenerateAsync(ctx, storage, submissionId);

        var thrown = await act.Should()
            .ThrowAsync<BusinessRuleViolationException>();

        thrown.Which.Message.Should().Contain("Study Tagging File");
        thrown.Which.Message.Should().Contain("m4-2-3-toxicology");

        // The refusal changed category in EPIC-019: it was a domain-capability
        // gap ("RegOS does not record studies") and is now data completeness —
        // a fact a user supplies on the content plan.
        thrown.Which.Message.Should().Contain("content plan");

        // Not confused with the other two refusals, nor with the keyed node.
        thrown.Which.Message.Should().NotContain("eCTD token");
        thrown.Which.Message.Should().NotContain("has not been read");
    }

    /// <summary>
    /// <b>The whole of EPIC-019, as one package.</b> A document in 4.2.3 that
    /// names a study now generates rather than refusing — and what it generates
    /// is checked by two oracles that answer different questions.
    /// </summary>
    /// <remarks>
    /// <c>xmllint</c> answers <i>is this legal?</i> against ICH's own DTD, which
    /// declares <c>file-tag/@name</c> as <c>CDATA</c> and would accept a
    /// misspelling. The stylesheet answers <i>is this a word?</i> by resolving
    /// every tag against <c>valid-values.xml</c> (E34). Neither alone is enough.
    /// </remarks>
    [Fact]
    public async Task AStudyReportDocument_ProducesAStudyTaggingFile_BothOraclesAgree()
    {
        await using var ctx = New();

        var study = await AStudyAsync(ctx, "TOX-2024-001");

        var (submissionId, storage) = await APublishedEctdSequenceAsync(
            ctx,
            sectionCode: "4.2.3",
            study: study,
            fileTag: "pre-clinical-study-report");

        var generated = await GenerateAsync(ctx, storage, submissionId);

        generated.StudyTaggingFiles.Should().ContainSingle()
            .Which.Should().EndWith("stf-tox-2024-001.xml");

        var stf = Path.Combine(
            generated.RootPath,
            generated.StudyTaggingFiles[0]
                .Replace('/', Path.DirectorySeparatorChar));

        File.Exists(stf).Should().BeTrue();

        // Oracle one — structure. The DTD ships in the package's own util/dtd/,
        // so this is the file checking itself against what it declares.
        var structure = Xmllint(stf);
        structure.ExitCode.Should().Be(0, structure.Output);

        // Oracle two — vocabulary. Zero red rows means every tag, category and
        // property resolved against ICH's published list.
        var stylesheet = Path.Combine(
            generated.RootPath, "util", "style", "ich-stf-stylesheet-2-3.xsl");

        File.Exists(stylesheet).Should().BeTrue(
            "the stylesheet and valid-values.xml travel together (E34)");

        RedRowsFrom(stf, stylesheet).Should().Be(0);

        // And the backbone still validates with the STF's leaf in it.
        ValidateIndex(generated.RootPath).ExitCode.Should().Be(0);

        var xml = await File.ReadAllTextAsync(stf);

        xml.Should().Contain("<study-id>TOX-2024-001</study-id>");
        xml.Should().Contain("pre-clinical-study-report");
        xml.Should().Contain("info-type=\"ich\"");

        // It carries no file — every doc-content points at a leaf the backbone
        // already holds (ADR-054).
        xml.Should().Contain("index.xml#");
        xml.Should().NotContain(".pdf");
    }

    /// <summary>
    /// <b>The oracle that the first one cannot be.</b> A misspelled tag is
    /// structurally valid and semantically empty; this is the assertion that
    /// says so out loud.
    /// </summary>
    [Fact]
    public async Task AMisspelledFileTag_PassesTheDtd_AndTheStylesheetCatchesIt()
    {
        await using var ctx = New();

        var study = await AStudyAsync(ctx, "TOX-2024-002");

        var (submissionId, storage) = await APublishedEctdSequenceAsync(
            ctx,
            sectionCode: "4.2.3",
            study: study,
            // Written straight onto the aggregate, which takes the token as
            // given — the handler is what refuses this in the product, and
            // this test is about what the package would say if it did not.
            fileTag: "pre-clinical-study-report");

        var generated = await GenerateAsync(ctx, storage, submissionId);

        var stf = Path.Combine(
            generated.RootPath,
            generated.StudyTaggingFiles[0]
                .Replace('/', Path.DirectorySeparatorChar));

        var corrupted = (await File.ReadAllTextAsync(stf))
            .Replace("pre-clinical-study-report", "pre-clinical-studdy-report");

        var probe = Path.Combine(Path.GetDirectoryName(stf)!, "stf-probe.xml");
        await File.WriteAllTextAsync(probe, corrupted);

        Xmllint(probe).ExitCode.Should().Be(0,
            "the DTD declares file-tag/@name as CDATA, so a misspelling is "
            + "perfectly legal XML (E34)");

        var stylesheet = Path.Combine(
            generated.RootPath, "util", "style", "ich-stf-stylesheet-2-3.xsl");

        RedRowsFrom(probe, stylesheet).Should().Be(1,
            "the stylesheet resolves every tag against valid-values.xml and "
            + "paints the ones ICH does not publish");
    }

    /// <summary>
    /// The freeze boundary, as the only thing that could demonstrate it:
    /// <c>Study (mutable) → Publication → frozen snapshot → STF XML</c>.
    /// </summary>
    [Fact]
    public async Task RenamingAStudyAfterFiling_DoesNotChangeWhatTheSequenceSaid()
    {
        await using var ctx = New();

        var study = await AStudyAsync(ctx, "TOX-2024-003");

        var (submissionId, storage) = await APublishedEctdSequenceAsync(
            ctx, sectionCode: "4.2.3", study: study, fileTag: "synopsis");

        var before = await GenerateAsync(ctx, storage, submissionId);
        var beforeXml = await File.ReadAllTextAsync(Path.Combine(
            before.RootPath,
            before.StudyTaggingFiles[0].Replace('/', Path.DirectorySeparatorChar)));

        // The registry moves on. The filed sequence does not.
        var tracked = await ctx.NonClinicalStudies.SingleAsync(
            s => s.Id == study.Id);

        tracked.Retitle("A Completely Different Title");
        await ctx.SaveChangesAsync();

        var after = await GenerateAsync(ctx, storage, submissionId);
        var afterXml = await File.ReadAllTextAsync(Path.Combine(
            after.RootPath,
            after.StudyTaggingFiles[0].Replace('/', Path.DirectorySeparatorChar)));

        afterXml.Should().Be(beforeXml,
            "an STF is projected from what the sequence filed, never from the "
            + "registry — regenerating 0000 must reproduce what FDA received");

        afterXml.Should().Contain("A 13-Week Oral Toxicity Study In Rats");
        afterXml.Should().NotContain("A Completely Different Title");
    }

    /// <summary>
    /// <b>The bounds are FDA's, not the module's.</b> §2.8 enumerates 4.2.x and
    /// 5.3.1.x–5.3.5.x: 5.2 is exempt by name and bare 5.3 is outside the range,
    /// and both hold documents in the seeded blueprint — 5.3 has a
    /// <em>required</em> one.
    /// </summary>
    /// <remarks>
    /// A rule that refused all of Modules 4 and 5 would pass the test above and
    /// still be wrong, so the boundary is asserted rather than the interior.
    /// </remarks>
    [Theory]
    [InlineData("5.2")]
    [InlineData("5.3")]
    public async Task ASectionOutsideTheStudyTaggedRange_StillRenders(
        string sectionCode)
    {
        await using var ctx = New();
        var (submissionId, storage) =
            await APublishedEctdSequenceAsync(ctx, sectionCode: sectionCode);

        var generated = await GenerateAsync(ctx, storage, submissionId);

        generated.Leaves.Should().ContainSingle();
        ValidateIndex(generated.RootPath).ExitCode.Should().Be(0);
    }

    /// <summary>
    /// <b>ADR-045's central claim, as something that can fail.</b> A RegOS
    /// sequence holds the whole dossier; an eCTD sequence holds only what
    /// changed, and the DTD has no <c>unchanged</c> operation. So a carried-
    /// forward document produces no leaf — and no file either, because a file
    /// the backbone never names is a file a regulator has to ask about.
    /// </summary>
    [Fact]
    public async Task ACarriedForwardDocument_ProducesNeitherALeafNorAFile()
    {
        await using var ctx = New();

        var (firstId, storage) =
            await APublishedEctdSequenceAsync(ctx, sectionCode: "2.3");

        var secondId = await ASequenceCarryingTheSameDocumentAsync(ctx, firstId);

        var generated = await GenerateAsync(ctx, storage, secondId);

        generated.Leaves.Should().BeEmpty();

        Directory.Exists(Path.Combine(generated.RootPath, "m2"))
            .Should().BeFalse();

        var xml = await File.ReadAllTextAsync(
            Path.Combine(generated.RootPath, "index.xml"));

        // One leaf, and it is the regional cross-link every sequence carries —
        // the carried-forward document contributes none. Asserted as a count
        // rather than an absence, because "no leaf at all" stopped being true
        // when S006 wired the cross-link.
        Regex.Matches(xml, "<leaf ").Should().ContainSingle();
        xml.Should().Contain("us-regional.xml");

        ValidateIndex(generated.RootPath).ExitCode.Should().Be(0);
    }

    /// <summary>
    /// <b>E22 — FDA accepts no path longer than 150 characters</b> (eCTD
    /// Technical Conformance Guide §2.4), where ICH Appendix 2 allows 230. The
    /// stricter of two published limits wins, and RegOS previously checked
    /// neither.
    /// </summary>
    /// <remarks>
    /// Refused rather than truncated: shortening would silently rename a
    /// document inside a package a regulator reads.
    /// </remarks>
    [Fact]
    public async Task APathLongerThanFdaAccepts_IsRefusedRatherThanShortened()
    {
        await using var ctx = New();

        // 2.3's folder is m2/23-qos, so the file name carries the length.
        var (submissionId, storage) = await APublishedEctdSequenceAsync(
            ctx, sectionCode: "2.3", documentFileName: new string('a', 160) + ".pdf");

        var act = async () => await GenerateAsync(ctx, storage, submissionId);

        var thrown = await act.Should()
            .ThrowAsync<BusinessRuleViolationException>();

        thrown.Which.Message.Should().Contain("no path longer than 150");
    }

    /// <summary>
    /// The fixed paths every package carries, measured rather than assumed —
    /// <c>util/dtd/us-regional-v3-3.dtd</c> is the longest of them.
    /// </summary>
    [Fact]
    public async Task ThePackagesOwnFiles_FitWithinTheLimit()
    {
        await using var ctx = New();
        var (submissionId, storage) =
            await APublishedEctdSequenceAsync(ctx, sectionCode: "2.3");

        var generated = await GenerateAsync(ctx, storage, submissionId);

        foreach (var path in generated.UtilityFiles.Concat(generated.BackboneFiles))
            $"0000/{path}".Length.Should().BeLessThanOrEqualTo(150);
    }

    // --- S007: delivery -------------------------------------------------------

    /// <summary>
    /// <b>ADR-049 as a signature rather than a promise.</b> The archive is
    /// handed to the caller and kept nowhere — no aggregate, no id, no status,
    /// and no scratch folder left on disk.
    /// </summary>
    [Fact]
    public async Task ThePackage_IsAnArchiveOfTheSequence_AndNothingIsLeftBehind()
    {
        await using var ctx = New();
        var (submissionId, storage) = await APublishedEctdSequenceAsync(ctx);

        var package = await new SequencePackageAssembler(
            new SequenceFolderGenerator(ctx, storage)).AssembleAsync(submissionId);

        package.FileName.Should().Be("0000.zip");

        using var archive = new ZipArchive(new MemoryStream(package.Contents));

        // The sequence folder is at the archive root: unpacking produces 0000/
        // and nothing above it. RegOS does not invent the application folder —
        // the mapping draws one and marks it "e.g.".
        archive.Entries.Select(x => x.FullName).Should().Contain(
        [
            "0000/index.xml",
            "0000/index-md5.txt",
            "0000/m1/us/us-regional.xml",
            "0000/util/dtd/ich-ectd-3-2.dtd",
            "0000/m1/us/12-cover-letters/cover-letter.pdf",
        ]);

        Directory.Exists(Path.Combine(Path.GetTempPath(), "regos-ectd",
            Path.GetFileNameWithoutExtension(package.FileName)))
            .Should().BeFalse();
    }

    /// <summary>
    /// <b>A refusal leaves nothing on disk either.</b> Everything is checked
    /// before a byte is written, and the scratch folder is removed whether or
    /// not an archive came out of it — a half-built directory that looks like a
    /// package is worse than no directory at all.
    /// </summary>
    [Fact]
    public async Task ARefusedPackage_LeavesNoScratchFolder()
    {
        await using var ctx = New();
        var (submissionId, storage) =
            await APublishedEctdSequenceAsync(ctx, sectionCode: "4.2.3");

        var before = ScratchFolders();

        var act = async () => await new SequencePackageAssembler(
            new SequenceFolderGenerator(ctx, storage)).AssembleAsync(submissionId);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();

        ScratchFolders().Should().BeEquivalentTo(before);
    }

    private static IReadOnlyList<string> ScratchFolders()
    {
        var root = Path.Combine(Path.GetTempPath(), "regos-ectd");

        return Directory.Exists(root) ? Directory.GetDirectories(root) : [];
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
        SubmissionFormat format = SubmissionFormat.Ectd,
        string sectionCode = "1.2",
        string documentFileName = "Cover Letter.pdf",
        NonClinicalStudy? study = null,
        string? fileTag = null)
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
            originalFileName: documentFileName,
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

        var section = await SectionAsync(ctx, sectionCode);

        var submission = SubmissionAggregate.Create(
            TestTenant.Id, applicationId, $"S004 {Guid.NewGuid()}", format,
            TestSubmissionClassification.Opens(),
            await BoundVersionAsync(ctx));

        var placement = submission.AttachDocument(
            document.Id, document.CurrentVersionId!.Value, section);

        if (study is not null)
        {
            submission.ReportNonClinicalStudy(
                placement.Id, study.Id, fileTag);
        }

        // FDA will not accept a filing with no regulatory contact, and the
        // contact needs a number whose kind is known and an address. Named
        // before publication because AssignRole freezes with everything else
        // (ADR-048).
        submission.AssignRole(
            await RegulatoryContactAsync(ctx), RegulatoryContactRole);

        if (publish)
        {
            // The snapshot the STF is projected from: what the study is called
            // now, frozen by the act of filing.
            submission.Publish(
                0,
                null,
                [],
                DateTimeOffset.UtcNow,
                study is null
                    ? []
                    : [new PublishedStudy(
                        study.Id.Value,
                        study.SponsorStudyIdentifier,
                        study.Title)]);
        }

        ctx.Submissions.Add(submission);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(submission.Id.Value);

        return (submission.Id, storage);
    }

    /// <summary>
    /// A second sequence holding exactly what the first held — same document,
    /// same section, same version. Under ADR-045 that is the normal case rather
    /// than a contrived one: a cumulative dossier repeats itself, and the
    /// increment is what the renderer has to find.
    /// </summary>
    private async Task<SubmissionId> ASequenceCarryingTheSameDocumentAsync(
        RegOSDbContext ctx, SubmissionId firstId)
    {
        var first = await ctx.Submissions.AsNoTracking()
            .Include(s => s.Documents)
            .SingleAsync(s => s.Id == firstId);

        var carried = first.Documents.Single();

        var second = SubmissionAggregate.Create(
            TestTenant.Id, first.ApplicationId, $"S005 {Guid.NewGuid()}",
            SubmissionFormat.Ectd, TestSubmissionClassification.Opens(),
            first.BoundTemplateVersionId);

        second.AttachDocument(
            carried.ProductDocumentId,
            carried.DocumentVersionId,
            carried.TemplateSectionId!.Value);

        // Every sequence is filed, so every sequence names its own contact —
        // ADR-048's whole point is that this is not an application-level fact.
        second.AssignRole(
            await RegulatoryContactAsync(ctx), RegulatoryContactRole);

        second.Publish(
            1,
            previousPublishedSequenceNumber: 0,
            [
                new PublishedPlacement(
                    carried.Id,
                    carried.ProductDocumentId,
                    carried.TemplateSectionId!.Value,
                    carried.DocumentVersionId),
            ],
            DateTimeOffset.UtcNow);

        ctx.Submissions.Add(second);
        await ctx.SaveChangesAsync();
        _submissionIds.Add(second.Id.Value);

        return second.Id;
    }

    /// <summary>
    /// A findable regulatory contact, reachable the way FDA requires — one
    /// telephone number whose kind is known, and one email address.
    /// </summary>
    /// <remarks>
    /// Created once and shared, like the application fixture: parallel test
    /// classes must converge on one row rather than race.
    /// </remarks>
    private static async Task<ContactId> RegulatoryContactAsync(RegOSDbContext ctx)
    {
        const string surname = "Fixture-Regulatory";

        var existing = await ctx.Contacts
            .AsNoTracking()
            .Where(x => x.LastName == surname)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (existing is not null)
            return existing;

        var organizationId = await ctx.Organizations
            .AsNoTracking().Select(x => x.Id).FirstAsync();

        var contact = Contact.Create(
            TestTenant.Id, organizationId, "Priya", surname,
            DateOnly.FromDateTime(DateTime.UtcNow));

        contact.AddRole(RegulatoryContactRole);
        contact.AddEmail("priya.regulatory@example.com");
        contact.AddPhone("+1 240 555 0100", ContactPhoneKind.Business);

        ctx.Contacts.Add(contact);

        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ctx.ChangeTracker.Clear();
        }

        return await ctx.Contacts
            .AsNoTracking()
            .Where(x => x.LastName == surname)
            .Select(x => x.Id)
            .FirstAsync();
    }

    /// <summary>REG — the only role with an FDA translation (E31).</summary>
    private static readonly ContactRoleId RegulatoryContactRole =
        new(Guid.Parse("81000000-0000-0000-0000-000000000003"));

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
        SectionAsync(RegOSDbContext ctx, string code)
    {
        var versionId = await BoundVersionAsync(ctx);

        return (await ctx.RegulatoryTemplates.AsNoTracking()
            .Include(t => t.Versions).ThenInclude(v => v.Sections)
            .SelectMany(t => t.Versions)
            .Where(v => v.Id == versionId)
            .SelectMany(v => v.Sections)
            .FirstAsync(s => s.Code == code)).Id;
    }

    /// <summary>
    /// Runs the package's own <c>index.xml</c> past xmllint, in place — so the
    /// relative DOCTYPE resolves against the <c>util/dtd/</c> the generator
    /// wrote, not against a copy arranged by the test.
    /// </summary>
    /// <summary>
    /// <c>xmllint</c> against whatever DOCTYPE the file names, resolved from the
    /// folder it sits in — so a package's own <c>util/dtd/</c> is what checks it.
    /// </summary>
    private static (int ExitCode, string Output) Xmllint(string path)
    {
        using var process = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo("xmllint")
            {
                ArgumentList = { "--noout", "--valid", path },
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            })!;

        var output = process.StandardError.ReadToEnd()
            + process.StandardOutput.ReadToEnd();

        process.WaitForExit();

        return (process.ExitCode, output);
    }

    /// <summary>
    /// A registered non-clinical study, the Module 4 half.
    /// </summary>
    private async Task<NonClinicalStudy> AStudyAsync(
        RegOSDbContext ctx, string? identifier = null)
    {
        var study = NonClinicalStudy.Register(
            TestTenant.Id,
            identifier ?? $"TOX-{Guid.NewGuid():N}"[..20],
            "A 13-Week Oral Toxicity Study In Rats");

        ctx.NonClinicalStudies.Add(study);
        await ctx.SaveChangesAsync();
        _studyIds.Add(study.Id.Value);

        return study;
    }

    private readonly List<Guid> _studyIds = [];

    /// <summary>
    /// The vocabulary oracle (E34). <c>xmllint</c> answers <i>is this legal?</i>
    /// and says yes to <c>sinopsis</c>; the ICH stylesheet answers <i>is this a
    /// word?</i> by resolving every tag against <c>valid-values.xml</c> and
    /// painting unknown ones <c>#FF6666</c>.
    /// </summary>
    private static int RedRowsFrom(string stfPath, string stylesheetPath)
    {
        using var process = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo("xsltproc")
            {
                ArgumentList = { stylesheetPath, stfPath },
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            })!;

        var html = process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();

        return html.Split("FF6666").Length - 1;
    }

    private static (int ExitCode, string Output) ValidateIndex(string root) =>
        Xmllint(Path.Combine(root, IchBackboneRenderer.FileName));

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        await ctx.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Submissions\" WHERE \"Id\" = ANY({0})",
            _submissionIds.ToArray());
        await ctx.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"NonClinicalStudies\" WHERE \"Id\" = ANY({0})",
            _studyIds.ToArray());
        await ctx.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"ProductDocuments\" WHERE \"Id\" = ANY({0})",
            _documentIds.ToArray());

        foreach (var root in _outputRoots.Where(Directory.Exists))
            Directory.Delete(root, recursive: true);
    }
}
