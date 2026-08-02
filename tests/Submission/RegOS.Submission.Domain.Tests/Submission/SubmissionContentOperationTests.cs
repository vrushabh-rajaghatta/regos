using FluentAssertions;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

/// <summary>
/// What a filing did to the sequence before it (ADR-045).
/// </summary>
/// <remarks>
/// The identity that survives across sequences is <b>(document, section)</b>.
/// These tests are written in those terms deliberately: every interesting case
/// here is one where the version, the section or the document differs, and the
/// rule reads the key rather than guessing at intent.
/// </remarks>
public class SubmissionContentOperationTests
{
    private static readonly DateTimeOffset PublishedAt =
        new(2026, 8, 2, 9, 0, 0, TimeSpan.Zero);

    private static readonly TemplateSectionId Module32S2 =
        new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"));
    private static readonly TemplateSectionId Module11 =
        new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"));

    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(
            TenantId.New(),
            new RegulatoryApplicationId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Protocol amendment",
            SubmissionFormat.Ectd);

    // --- The first filing ----------------------------------------------------

    /// <summary>
    /// The empty-baseline case, asserted so it can never become an accident:
    /// with nothing behind it, every placement in a first filing is new.
    /// </summary>
    [Fact]
    public void TheFirstSequence_IsAllNew()
    {
        var submission = NewDraft();
        var a = Place(submission, Document(1), Version(1), Module32S2);
        var b = Place(submission, Document(2), Version(2), Module11);

        submission.Publish(0, null, [], PublishedAt);

        submission.Documents.Should().OnlyContain(
            d => d.Operation == SubmissionContentOperation.New);
        a.ReplacesSubmissionDocumentId.Should().BeNull();
        b.ReplacesSubmissionDocumentId.Should().BeNull();
        submission.Deletions.Should().BeEmpty();
    }

    [Fact]
    public void AFirstSequenceGivenABaseline_IsRefused()
    {
        var submission = NewDraft();
        Place(submission, Document(1), Version(1), Module32S2);

        var act = () => submission.Publish(
            0, null, [Placement(Document(1), Version(1), Module32S2)], PublishedAt);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.FirstSequenceHasNoBaseline);
    }

    // --- Against a baseline --------------------------------------------------

    [Fact]
    public void SameDocumentSameSectionSameVersion_IsUnchanged()
    {
        var document = Document(1);
        var version = Version(1);
        var submission = NewDraft();
        var placed = Place(submission, document, version, Module32S2);

        submission.Publish(
            1, 0, [Placement(document, version, Module32S2)], PublishedAt);

        placed.Operation.Should().Be(SubmissionContentOperation.Unchanged);
        // Unchanged is not a replacement of itself.
        placed.ReplacesSubmissionDocumentId.Should().BeNull();
        submission.Deletions.Should().BeEmpty();
    }

    [Fact]
    public void SameDocumentSameSectionNewVersion_IsReplace_AndPointsAtWhatItSuperseded()
    {
        var document = Document(1);
        var submission = NewDraft();
        var placed = Place(submission, document, Version(2), Module32S2);
        var previous = Placement(document, Version(1), Module32S2);

        submission.Publish(1, 0, [previous], PublishedAt);

        placed.Operation.Should().Be(SubmissionContentOperation.Replace);
        // eCTD's modified-file: the specific prior leaf, not merely "the one
        // before". Derivable only here, and meaningless afterwards without it.
        placed.ReplacesSubmissionDocumentId.Should().Be(previous.Id);
    }

    [Fact]
    public void ADocumentTheBaselineNeverCarried_IsNew()
    {
        var submission = NewDraft();
        var placed = Place(submission, Document(2), Version(9), Module11);

        submission.Publish(
            1, 0, [Placement(Document(1), Version(1), Module32S2)], PublishedAt);

        placed.Operation.Should().Be(SubmissionContentOperation.New);
        placed.ReplacesSubmissionDocumentId.Should().BeNull();
    }

    /// <summary>
    /// The cumulative model's defining case: a filing carries the whole dossier,
    /// so a placement it does <em>not</em> carry is one it withdrew.
    /// </summary>
    [Fact]
    public void APlacementTheBaselineHadAndThisFilingDoesNot_IsDeleted()
    {
        var withdrawn = Placement(Document(1), Version(1), Module32S2);
        var submission = NewDraft();
        Place(submission, Document(2), Version(1), Module11);

        submission.Publish(1, 0, [withdrawn], PublishedAt);

        submission.Deletions.Should().ContainSingle();
        var deletion = submission.Deletions.Single();
        deletion.ProductDocumentId.Should().Be(withdrawn.ProductDocumentId);
        deletion.TemplateSectionId.Should().Be(Module32S2);
        deletion.DeletesSubmissionDocumentId.Should().Be(withdrawn.Id);
    }

    /// <summary>
    /// A document that moved section reads as a delete plus a new, because that
    /// is what the key says happened. **Whether a regulator would call it a
    /// replace is an open question** (EPIC-004 hypothesis 4, resolved at
    /// EPIC-007) — this test pins today's answer so a later change to it is a
    /// deliberate act rather than a drift.
    /// </summary>
    [Fact]
    public void ADocumentThatMovedSection_ReadsAsADeleteAndANew()
    {
        var document = Document(1);
        var version = Version(1);
        var submission = NewDraft();
        var placed = Place(submission, document, version, Module11);

        submission.Publish(
            1, 0, [Placement(document, version, Module32S2)], PublishedAt);

        placed.Operation.Should().Be(SubmissionContentOperation.New);
        submission.Deletions.Should().ContainSingle()
            .Which.TemplateSectionId.Should().Be(Module32S2);
    }

    /// <summary>
    /// An operation is a fact about a placement. A document sitting nowhere is
    /// in no section, produces no leaf, and did nothing to the previous
    /// sequence — so it keeps a null operation even after publication.
    /// </summary>
    [Fact]
    public void AnUnplacedAttachment_HasNoOperation()
    {
        var submission = NewDraft();
        var loose = submission.AttachDocument(Document(3), Version(1));

        submission.Publish(1, 0, [], PublishedAt);

        loose.Operation.Should().BeNull();
        submission.Deletions.Should().BeEmpty();
    }

    [Fact]
    public void ADraft_HasNoOperations()
    {
        var submission = NewDraft();
        var placed = Place(submission, Document(1), Version(1), Module32S2);

        placed.Operation.Should().BeNull();
        submission.Deletions.Should().BeEmpty();
    }

    /// <summary>
    /// Nothing derives Append. It exists so that a later story adds a rule
    /// rather than a migration (EPIC-004 hypothesis 5).
    /// </summary>
    [Fact]
    public void NothingProducesAppend()
    {
        var document = Document(1);
        var submission = NewDraft();
        Place(submission, document, Version(2), Module32S2);

        submission.Publish(
            1, 0, [Placement(document, Version(1), Module32S2)], PublishedAt);

        submission.Documents.Should().NotContain(
            d => d.Operation == SubmissionContentOperation.Append);
    }

    // --- Helpers -------------------------------------------------------------

    // --- Format does not reach the derivation --------------------------------

    /// <summary>
    /// <b>The delta is domain; the format is rendering</b> (ADR-047 decision 4).
    /// </summary>
    /// <remarks>
    /// A paper sequence still changed something relative to the one before it —
    /// it simply renders as a cover letter listing the changes rather than an
    /// XML backbone. This is asserted rather than assumed because the obvious
    /// future "simplification" is to skip derivation for anything that is not
    /// eCTD, and that would quietly turn ADR-045's cumulative model from the
    /// product thesis into an eCTD implementation detail.
    /// </remarks>
    [Theory]
    [InlineData(SubmissionFormat.Ectd)]
    [InlineData(SubmissionFormat.Nees)]
    [InlineData(SubmissionFormat.Paper)]
    public void OperationDerivation_IsIndependentOfFormat(SubmissionFormat format)
    {
        var carried = Document(1);
        var replaced = Document(2);
        var added = Document(3);
        var withdrawn = Document(4);

        var submission = SubmissionAggregate.Create(
            TenantId.New(),
            new RegulatoryApplicationId(Guid.NewGuid()),
            new SubmissionTypeId(Guid.NewGuid()),
            "Protocol amendment",
            format);

        var unchanged = Place(submission, carried, Version(1), Module32S2);
        var replacement = Place(submission, replaced, Version(9), Module11);
        var addition = Place(submission, added, Version(1), Module11);

        var previous = Placement(replaced, Version(2), Module11);

        submission.Publish(
            1,
            0,
            [
                Placement(carried, Version(1), Module32S2),
                previous,
                Placement(withdrawn, Version(1), Module32S2)
            ],
            PublishedAt);

        // Every operation the eCTD case produces, produced identically.
        unchanged.Operation.Should().Be(SubmissionContentOperation.Unchanged);
        replacement.Operation.Should().Be(SubmissionContentOperation.Replace);
        replacement.ReplacesSubmissionDocumentId.Should().Be(previous.Id);
        addition.Operation.Should().Be(SubmissionContentOperation.New);

        submission.Deletions.Should().ContainSingle()
            .Which.ProductDocumentId.Should().Be(withdrawn);

        submission.Format.Should().Be(format);
    }

    private static SubmissionDocument Place(
        SubmissionAggregate submission,
        ProductDocumentId document,
        DocumentVersionId version,
        TemplateSectionId section) =>
        submission.AttachDocument(document, version, section);

    private static PublishedPlacement Placement(
        ProductDocumentId document,
        DocumentVersionId version,
        TemplateSectionId section) =>
        new(SubmissionDocumentId.New(), document, section, version);

    private static ProductDocumentId Document(int n) =>
        new(Guid.Parse($"bbbbbbbb-0000-0000-0000-{n:D12}"));

    private static DocumentVersionId Version(int n) =>
        new(Guid.Parse($"cccccccc-0000-0000-0000-{n:D12}"));
}
