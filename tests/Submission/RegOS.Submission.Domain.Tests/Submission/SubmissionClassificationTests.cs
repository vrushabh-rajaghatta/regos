using FluentAssertions;

using RegOS.ReferenceData.Domain.SubmissionSubType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

/// <summary>
/// <b>EPIC-007a S003 — which regulatory activity a sequence belongs to.</b>
///
/// The design named four invariants. Only three are tested here, and the
/// missing one is the point: <i>a submission cannot both open an activity and
/// continue one</i> has no test because
/// <see cref="SubmissionClassification"/> gives it no way to be expressed — the
/// compiler is the assertion. What the database does about rows that never pass
/// through C# is a CHECK constraint, and it is verified against a real Postgres.
/// </summary>
public class SubmissionClassificationTests
{
    private static readonly RegulatoryApplicationId TheApplication =
        new(Guid.NewGuid());

    private static readonly SubmissionTypeId AnnualReport =
        SubmissionTypeId.New();

    private static readonly SubmissionSubTypeId Amendment =
        SubmissionSubTypeId.New();

    private static SubmissionAggregate Draft(
        SubmissionClassification classification,
        RegulatoryApplicationId? application = null) =>
        SubmissionAggregate.Create(
            TenantId.New(),
            application ?? TheApplication,
            "Sequence",
            SubmissionFormat.Ectd,
            classification);

    /// <summary>A published opener, which is the only legal origin.</summary>
    private static OriginatingSubmission APublishedOpener(
        RegulatoryApplicationId? application = null) =>
        new(SubmissionId.New(), application ?? TheApplication, 0, true);

    // --- Opening an activity --------------------------------------------------

    [Fact]
    public void ASequenceThatOpensAnActivity_CarriesItsTypeAndNoOrigin()
    {
        var submission = Draft(
            SubmissionClassification.Opens(AnnualReport, Amendment));

        submission.SubmissionTypeId.Should().Be(AnnualReport);
        submission.OriginatingSubmissionId.Should().BeNull();
        submission.SubmissionSubTypeId.Should().Be(Amendment);
        submission.IsClassified.Should().BeTrue();
    }

    /// <summary>
    /// The activity's type lives on the sequence that opened it, and nowhere
    /// else. Two copies of one fact can only differ by one being wrong.
    /// </summary>
    [Fact]
    public void ASequenceThatContinuesAnActivity_CarriesNoTypeOfItsOwn()
    {
        var origin = APublishedOpener();

        var submission = Draft(
            SubmissionClassification.Continues(origin, Amendment));

        submission.OriginatingSubmissionId.Should().Be(origin.Id);
        submission.SubmissionTypeId.Should().BeNull();
    }

    // --- Invariant 2: one activity, one application ---------------------------

    [Fact]
    public void AnOriginInAnotherApplication_IsRefused()
    {
        var elsewhere = APublishedOpener(new RegulatoryApplicationId(Guid.NewGuid()));

        var act = () => Draft(
            SubmissionClassification.Continues(elsewhere, Amendment));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.OriginatingSubmissionDifferentApplication);
    }

    // --- Invariant 3: the origin is published ---------------------------------

    /// <summary>
    /// eCTD identifies an activity by the opener's sequence number, and ADR-044
    /// assigns that number at publish. A draft origin would leave
    /// <c>submission-id</c> with nothing to write.
    /// </summary>
    [Fact]
    public void AnUnpublishedOrigin_IsRefused()
    {
        var draftOrigin = new OriginatingSubmission(
            SubmissionId.New(), TheApplication, SequenceNumber: null, IsItselfAnOrigin: true);

        var act = () => Draft(
            SubmissionClassification.Continues(draftOrigin, Amendment));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.OriginatingSubmissionNotPublished);
    }

    [Fact]
    public void SequenceZero_CountsAsPublished()
    {
        // Regression guard: 0000 is a legal sequence number (evidence E4), and
        // a null check written as a falsiness check would reject it.
        var act = () => Draft(
            SubmissionClassification.Continues(APublishedOpener(), Amendment));

        act.Should().NotThrow();
    }

    // --- Invariant 4: the origin is itself an origin ---------------------------

    /// <summary>
    /// FDA example #22 carries <c>submission-id="0001"</c> — the opener's
    /// number, not the predecessor's. Allowing a chain would mean resolving it
    /// transitively at render time, so one cannot be built.
    /// </summary>
    [Fact]
    public void AnOriginThatIsItselfAContinuation_IsRefused()
    {
        var middleOfAChain = new OriginatingSubmission(
            SubmissionId.New(), TheApplication, SequenceNumber: 2, IsItselfAnOrigin: false);

        var act = () => Draft(
            SubmissionClassification.Continues(middleOfAChain, Amendment));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.OriginatingSubmissionIsNotAnOrigin);
    }

    // --- The sub-type is always required --------------------------------------

    [Fact]
    public void OpeningWithoutSayingWhatTheSequenceDoes_IsRefused()
    {
        var act = () => SubmissionClassification.Opens(AnnualReport, default);

        act.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.SubmissionSubTypeRequired);
    }

    [Fact]
    public void ContinuingWithoutSayingWhatTheSequenceDoes_IsRefused()
    {
        var act = () => SubmissionClassification.Continues(
            APublishedOpener(), default);

        act.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.SubmissionSubTypeRequired);
    }

    // --- Frozen at publication (ADR-047's mechanism) ---------------------------

    [Fact]
    public void ADraft_MayBeReclassified()
    {
        var submission = Draft(
            SubmissionClassification.Opens(AnnualReport, Amendment));

        var origin = APublishedOpener();

        submission.Reclassify(
            SubmissionClassification.Continues(origin, Amendment));

        submission.SubmissionTypeId.Should().BeNull();
        submission.OriginatingSubmissionId.Should().Be(origin.Id);
    }

    /// <summary>
    /// The draft guard <em>is</em> the freeze — no separate immutability
    /// mechanism, exactly as <c>ChangeFormat</c> does it (ADR-047).
    /// </summary>
    [Fact]
    public void APublishedSequence_CannotBeReclassified()
    {
        var submission = Draft(
            SubmissionClassification.Opens(AnnualReport, Amendment));

        submission.Publish(0, null, [], DateTimeOffset.UtcNow);

        var act = () => submission.Reclassify(
            SubmissionClassification.Opens(SubmissionTypeId.New(), Amendment));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.ClassificationLockedOncePublished);
    }

    /// <summary>
    /// Reclassifying a draft is held to the same rules as classifying it. A
    /// second entry point that skipped them would be a hole in three
    /// invariants at once.
    /// </summary>
    [Fact]
    public void ReclassifyingToAnIllegalOrigin_IsRefused()
    {
        var submission = Draft(
            SubmissionClassification.Opens(AnnualReport, Amendment));

        var elsewhere = APublishedOpener(new RegulatoryApplicationId(Guid.NewGuid()));

        var act = () => submission.Reclassify(
            SubmissionClassification.Continues(elsewhere, Amendment));

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.OriginatingSubmissionDifferentApplication);
    }
}
