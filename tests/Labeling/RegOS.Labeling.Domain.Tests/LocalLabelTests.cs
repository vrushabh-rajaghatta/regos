using FluentAssertions;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Tests;

public sealed class LocalLabelTests
{
    private static readonly TenantId Tenant = new(Guid.NewGuid());
    private static readonly MedicinalProductId Market = new(Guid.NewGuid());
    private static readonly DateTime Now = new(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc);

    private static LocalLabel ALabel(string type = "SMPC")
        => LocalLabel.Create(Tenant, Market, type, "ja", Now);

    private static LocalLabel AnApprovedLabel(
        DateOnly approvedOn,
        DateOnly effectiveFrom)
    {
        var label = ALabel();
        var draft = label.Draft!;

        label.PrepareRevision(
            draft.Id, ProductDocumentId.New(), null, null, null);

        label.PublishRevision(draft.Id, approvedOn, effectiveFrom);

        return label;
    }

    [Fact]
    public void ALabelIsBornWithItsFirstRevision()
    {
        var label = ALabel();

        label.Revisions.Should().HaveCount(1);
        label.Draft!.RevisionNumber.Should().Be(1);
        label.RevisionInForce.Should().BeNull();
    }

    [Fact]
    public void TheLabelCarriesNoCountry()
    {
        // The market-local tier already answers which jurisdiction this is
        // (ADR-039). A second copy could disagree with it.
        typeof(LocalLabel).GetProperties()
            .Select(x => x.Name)
            .Should().NotContain("CountryId");
    }

    [Fact]
    public void ArtworkIsAnOrdinaryLabelType()
    {
        // EPIC-018 D2: the same machinery, no special case. If this test ever
        // needs a branch to stay true, artwork has become a different thing.
        var artwork = ALabel("ARTWORK");

        artwork.LabelType.Code.Should().Be("ARTWORK");
        artwork.Draft!.RevisionNumber.Should().Be(1);
    }

    [Fact]
    public void TwoLabelsDoNotShareOneLabelTypeInstance()
    {
        var first = ALabel();
        var second = ALabel();

        first.LabelType.Should().Be(second.LabelType);
        first.LabelType.Should().NotBeSameAs(second.LabelType);
    }

    [Fact]
    public void AGlobalLabelTypeIsNotALocalOne()
    {
        // The two vocabularies are separate lists on purpose: a CCDS is not
        // something a market approves, and a carton is not a core position.
        var create = () => ALabel("CCDS");

        create.Should().Throw<DomainException>()
            .WithMessage(LocalLabelErrors.LabelTypeNotRecognised);
    }

    [Fact]
    public void ARevisionWithNoDocumentCannotBePutInForce()
    {
        var label = ALabel();

        var publish = () => label.PublishRevision(
            label.Draft!.Id,
            new DateOnly(2026, 5, 12),
            new DateOnly(2026, 6, 1));

        publish.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(LocalLabelErrors.ContentRequiredToPublish);
    }

    /// <summary>
    /// The local analogue of "publishing requires a document" — a statement
    /// about the artifact's truth, not a workflow step.
    /// </summary>
    [Fact]
    public void ApprovalAndEffectAreTwoFactsAndBothAreKept()
    {
        var label = AnApprovedLabel(
            new DateOnly(2026, 5, 12), new DateOnly(2026, 6, 1));

        var inForce = label.RevisionInForce!;

        inForce.ApprovedOn.Should().Be(new DateOnly(2026, 5, 12));
        inForce.EffectiveFrom.Should().Be(new DateOnly(2026, 6, 1));
    }

    [Fact]
    public void EffectiveImmediatelyIsOrdinary()
    {
        var sameDay = new DateOnly(2026, 5, 12);

        var label = AnApprovedLabel(sameDay, sameDay);

        label.RevisionInForce!.EffectiveFrom.Should().Be(sameDay);
    }

    [Fact]
    public void ARevisionCannotTakeEffectBeforeItWasApproved()
    {
        var label = ALabel();
        var draft = label.Draft!;

        label.PrepareRevision(
            draft.Id, ProductDocumentId.New(), null, null, null);

        var publish = () => label.PublishRevision(
            draft.Id,
            new DateOnly(2026, 5, 12),
            new DateOnly(2026, 5, 11));

        publish.Should().Throw<DomainException>()
            .WithMessage(LocalLabelErrors.EffectiveBeforeApproval);
    }

    /// <summary>
    /// The core invariant, and the reason the epic exists: a market issues a new
    /// revision while the global label stands still, and the one it replaces is
    /// retired in the same act.
    /// </summary>
    [Fact]
    public void ATranslationFixIsANewRevisionDerivedFromTheSameCoreVersion()
    {
        var coreVersion = GlobalLabelVersionId.New();

        var label = ALabel();
        var first = label.Draft!;

        label.PrepareRevision(
            first.Id, ProductDocumentId.New(), coreVersion, null, null);

        label.PublishRevision(
            first.Id, new DateOnly(2026, 5, 12), new DateOnly(2026, 6, 1));

        // Nothing changed globally. Japan issues the next revision anyway.
        var second = label.StartRevision();

        label.PrepareRevision(
            second.Id,
            ProductDocumentId.New(),
            coreVersion,
            null,
            "Translation correction.");

        label.PublishRevision(
            second.Id, new DateOnly(2026, 8, 20), new DateOnly(2026, 9, 1));

        label.RevisionInForce!.RevisionNumber.Should().Be(2);

        label.RevisionInForce.DerivedFromGlobalLabelVersionId
            .Should().Be(coreVersion);

        var retired = label.Revisions.Single(x => x.RevisionNumber == 1);

        retired.Status.Should().Be(LocalLabelRevisionStatus.Superseded);
        retired.EffectiveTo.Should().Be(new DateOnly(2026, 8, 31));

        // Both descend from the same core version, and the history is the
        // market's rather than the company's.
        retired.DerivedFromGlobalLabelVersionId.Should().Be(coreVersion);
    }

    [Fact]
    public void ARevisionMayDescendFromNoCoreVersionAtAll()
    {
        // A migrated portfolio does not know, and a local-first company holds
        // approved labelling before any core label exists here (D3).
        var label = AnApprovedLabel(
            new DateOnly(2026, 5, 12), new DateOnly(2026, 6, 1));

        label.RevisionInForce!.DerivedFromGlobalLabelVersionId
            .Should().BeNull();
    }

    [Fact]
    public void AnApprovedRevisionCannotBeChanged()
    {
        var label = AnApprovedLabel(
            new DateOnly(2026, 5, 12), new DateOnly(2026, 6, 1));

        var prepare = () => label.PrepareRevision(
            label.RevisionInForce!.Id,
            ProductDocumentId.New(),
            null,
            null,
            null);

        prepare.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(LocalLabelErrors.RevisionNotDraft);
    }

    [Fact]
    public void OnlyOneRevisionMayBePreparedAtATime()
    {
        var label = ALabel();

        var second = () => label.StartRevision();

        second.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(LocalLabelErrors.DraftAlreadyOpen);
    }

    [Fact]
    public void ADraftCanBeDiscardedAndItsNumberReissued()
    {
        var label = AnApprovedLabel(
            new DateOnly(2026, 5, 12), new DateOnly(2026, 6, 1));

        label.StartRevision();
        label.DiscardDraft();

        label.Draft.Should().BeNull();
        label.Revisions.Should().HaveCount(1);
        label.StartRevision().RevisionNumber.Should().Be(2);
    }

    [Fact]
    public void AnApprovedRevisionCannotBeDiscarded()
    {
        var label = AnApprovedLabel(
            new DateOnly(2026, 5, 12), new DateOnly(2026, 6, 1));

        var discard = () => label.DiscardDraft();

        discard.Should().Throw<NotFoundException>()
            .WithMessage(LocalLabelErrors.NoOpenDraft);
    }

    [Fact]
    public void TheArtworkCodeIsRecordedOnTheRevisionThatCarriesIt()
    {
        var label = ALabel("ARTWORK");
        var draft = label.Draft!;

        label.PrepareRevision(
            draft.Id, ProductDocumentId.New(), null, "  0123456789012  ", null);

        draft.DataCarrierCode.Should().Be("0123456789012");
    }
}
