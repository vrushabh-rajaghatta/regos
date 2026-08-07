using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Queries.GetSubmission;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;
using RegOS.Submission.Infrastructure.Services;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// <b>ADR-065 I1 — Regulatory Process is optional.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>This file is deliberately not in the Process test project</b>, and that is
/// the whole of its design. I1 is not <em>"Process accepts null"</em>; it is
/// <em>"RegOS works without Process"</em>, and a claim about RegOS-without-Process
/// proved from inside Process proves the wrong thing.
/// </para>
/// <para>
/// It follows that <b>this project references no Process project and must never
/// gain one.</b> <c>ProcessStepId</c> arrives transitively through
/// <c>Submission.Domain</c> and is never named here — the tests below read a
/// property and assert it is null, which is all a context that does not use
/// Process should ever need to know about it.
/// </para>
/// <para>
/// <b>Found missing by S008's evidence audit.</b> I1 had been true since S006 and
/// asserted nowhere: no test outside Process set or read a <c>ProcessStepId</c>,
/// so the invariant held because nobody had written the code that would break it.
/// <em>Nothing contradicts it</em> is not evidence.
/// </para>
/// </remarks>
[Collection(SubmissionDatabase.Collection)]
public sealed class SubmissionNeedsNoPlanTests : IAsyncLifetime
{
    private static readonly DocumentTypeId SeededCer =
        new(Guid.Parse("50000000-0000-0000-0000-000000000001"));

    private readonly SubmissionDatabase _database;

    private readonly List<Guid> _submissionIds = [];
    private readonly List<Guid> _documentIds = [];

    public SubmissionNeedsNoPlanTests(SubmissionDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// <b>The whole regulated lifecycle, with no plan anywhere.</b> Create,
    /// attach, validate, publish — and a published sequence is the point at
    /// which a submission becomes a regulatory fact, so nothing downstream of it
    /// can be said to have been skipped.
    /// </summary>
    [Fact]
    public async Task A_submission_is_created_validated_and_published_with_no_plan()
    {
        SubmissionId submissionId;

        await using (var seed = New())
        {
            var (applicationId, globalProductId) =
                await TestApplications.EnsureAsync(seed, "TEST-NOPLAN");

            var document = await ADocument(seed, globalProductId);

            submissionId = await ASubmission(seed, applicationId, document);
        }

        PublishSubmissionResult result;

        await using (var act = New())
        {
            result = await Publisher(act).HandleAsync(
                new PublishSubmissionCommand(submissionId), default);
        }

        result.Published.Should().BeTrue(
            "an empty Process schema changes nothing about filing a submission "
                + "— ADR-065 I1");

        await using var check = New();

        var published = await check.Submissions
            .AsNoTracking()
            .FirstAsync(x => x.Id == submissionId);

        published.Status.Should().Be(SubmissionStatus.Published);
        published.SequenceNumber.Should().NotBeNull(
            "it is a real, numbered sequence, not a partial record");
        published.ProcessStepId.Should().BeNull(
            "no plan was ever created, and its absence is the ordinary state");
    }

    /// <summary>
    /// <b>And nothing interprets the absence</b> (I9's half of I1). The
    /// validation engine is the one place a rule could quietly decide that an
    /// unlinked submission is incomplete.
    /// </summary>
    /// <remarks>
    /// Asserted by reading the issues rather than by counting them: a valid
    /// submission produces none at all, so the meaningful check is on the
    /// <em>invalid</em> one — where every issue raised has some other cause, and
    /// not one of them mentions a plan, a step or a process.
    /// </remarks>
    [Fact]
    public async Task No_validation_issue_is_about_the_absence_of_a_plan()
    {
        SubmissionId submissionId;

        await using (var seed = New())
        {
            var (applicationId, _) =
                await TestApplications.EnsureAsync(seed, "TEST-NOPLAN");

            // Deliberately not ready — so the validator has plenty to say, and
            // none of it is about Process.
            submissionId = await ASubmission(seed, applicationId, document: null);
        }

        await using var act = New();

        var result = await Publisher(act).HandleAsync(
            new PublishSubmissionCommand(submissionId), default);

        result.Published.Should().BeFalse();
        result.Validation!.Issues.Should().NotBeEmpty(
            "an unready submission has real problems — that is what makes this "
                + "a fair place to look");

        var mentions = result.Validation.Issues
            .Where(issue =>
                Mentions(issue.Message) || Mentions(issue.RuleCode))
            .ToList();

        mentions.Should().BeEmpty(
            "ADR-065 I1 — an unlinked submission is not an incomplete one. The "
                + "day a rule says otherwise, Process has stopped being optional "
                + "and become a thing every context must satisfy");

        static bool Mentions(string? text)
            => text is not null
                && (text.Contains("plan", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("step", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("process", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// <b>The read exposes the null and derives nothing from it.</b> A detail
    /// page for a submission that serves no planned work is a complete page.
    /// </summary>
    [Fact]
    public async Task The_read_returns_a_complete_submission_with_a_null_step()
    {
        SubmissionId submissionId;

        await using (var seed = New())
        {
            var (applicationId, _) =
                await TestApplications.EnsureAsync(seed, "TEST-NOPLAN");

            submissionId = await ASubmission(seed, applicationId, document: null);
        }

        await using var read = New();

        var detail = await new GetSubmissionHandler(read)
            .HandleAsync(submissionId, default);

        detail.Should().NotBeNull();
        detail!.ProcessStepId.Should().BeNull();

        detail.Title.Should().NotBeNullOrWhiteSpace();
        detail.ApplicationName.Should().NotBeNullOrWhiteSpace();
        detail.Status.Should().Be(nameof(SubmissionStatus.Draft));
        detail.NextSequenceNumber.Should().BeGreaterThanOrEqualTo(0,
            "everything the screen needs is present; the plan link is the only "
                + "thing missing, and it was never required");
    }

    // --- fixtures ------------------------------------------------------------

    private RegOSDbContext New() => new(_database.Options, TestTenant.Context);

    private static PublishSubmissionHandler Publisher(RegOSDbContext context)
        => new(
            new SubmissionValidator(new SubmissionRepository(context), context),
            new SubmissionPublicationBaseline(context),
            new SubmissionRepository(context),
            context);

    private async Task<ProductDocumentAggregate> ADocument(
        RegOSDbContext context, GlobalProductId globalProductId)
    {
        var document = ProductDocumentAggregate.Create(
            TestTenant.Id, globalProductId, SeededCer,
            "No-plan doc " + Guid.NewGuid());

        document.AddInitialVersion(
            originalFileName: "cer.pdf",
            storedFileName: "v1.pdf",
            contentType: "application/pdf",
            fileSize: 1024,
            storagePath:
                $"products/{globalProductId.Value}/{document.Id.Value}/v1.pdf",
            checksum: "sha256-x");

        document.Activate();

        context.ProductDocuments.Add(document);
        await context.SaveChangesAsync();

        _documentIds.Add(document.Id.Value);

        return document;
    }

    private async Task<SubmissionId> ASubmission(
        RegOSDbContext context,
        RegulatoryApplicationId applicationId,
        ProductDocumentAggregate? document)
    {
        var submission = SubmissionAggregate.Create(
            TestTenant.Id, applicationId, "No-plan sub " + Guid.NewGuid(),
            SubmissionFormat.Ectd,
            TestSubmissionClassification.Opens());

        if (document is not null)
            submission.AttachDocument(
                document.Id, document.CurrentVersionId!.Value);

        context.Submissions.Add(submission);
        await context.SaveChangesAsync();

        _submissionIds.Add(submission.Id.Value);

        return submission.Id;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var context = New();

        foreach (var id in _submissionIds)
        {
            var submission = await context.Submissions
                .Include(x => x.Documents)
                .Include(x => x.Deletions)
                .FirstOrDefaultAsync(x => x.Id == new SubmissionId(id));

            if (submission is not null)
                context.Submissions.Remove(submission);
        }

        await context.SaveChangesAsync();

        foreach (var id in _documentIds)
        {
            var document = await context.ProductDocuments
                .Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.Id == new ProductDocumentId(id));

            if (document is not null)
                context.ProductDocuments.Remove(document);
        }

        await context.SaveChangesAsync();
    }
}
