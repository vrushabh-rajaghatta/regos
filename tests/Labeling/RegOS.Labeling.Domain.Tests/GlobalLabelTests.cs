using FluentAssertions;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Labeling.Domain.Tests;

public sealed class GlobalLabelTests
{
    private static readonly TenantId Tenant = new(Guid.NewGuid());
    private static readonly GlobalProductId Product = new(Guid.NewGuid());
    private static readonly DateTime Now = new(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc);

    private static GlobalLabel ALabel()
        => GlobalLabel.Create(Tenant, Product, "Core data sheet", "CCDS", Now);

    private static GlobalLabel APublishedLabel(DateOnly effectiveFrom)
    {
        var label = ALabel();

        label.AttachContent(label.Draft!.Id, ProductDocumentId.New());
        label.PublishVersion(label.Draft!.Id, effectiveFrom, Now);

        return label;
    }

    [Fact]
    public void ALabelIsBornWithItsFirstDraft()
    {
        var label = ALabel();

        label.Versions.Should().HaveCount(1);
        label.Draft!.VersionNumber.Should().Be(1);
        label.VersionInForce.Should().BeNull();
    }

    [Fact]
    public void TheLabelTypeIsResolvedFromTheVocabularyAndCarriesItsSystem()
    {
        var label = ALabel();

        label.LabelType.Code.Should().Be("CCDS");
        label.LabelType.System.Should().Be("regos-internal");
    }

    /// <summary>
    /// The defect EPIC-010a S001 paid for, guarded one level up: an owned value
    /// object is tracked against exactly one owner, so two labels sharing one
    /// instance would persist nulls on the second.
    /// </summary>
    [Fact]
    public void TwoLabelsDoNotShareOneLabelTypeInstance()
    {
        var first = ALabel();
        var second = ALabel();

        first.LabelType.Should().Be(second.LabelType);
        first.LabelType.Should().NotBeSameAs(second.LabelType);
    }

    [Fact]
    public void AnUnknownLabelTypeIsRefused()
    {
        var create = () => GlobalLabel.Create(
            Tenant, Product, "Core data sheet", "NOT-A-TYPE", Now);

        create.Should().Throw<DomainException>()
            .WithMessage(GlobalLabelErrors.LabelTypeNotRecognised);
    }

    [Fact]
    public void OnlyOneDraftMayBeOpenAtATime()
    {
        var label = ALabel();

        var second = () => label.StartDraft();

        second.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(GlobalLabelErrors.DraftAlreadyOpen);
    }

    /// <summary>
    /// The rule that makes the content link load-bearing rather than decorative.
    /// </summary>
    [Fact]
    public void AVersionWithNoDocumentCannotBePublished()
    {
        var label = ALabel();

        var publish = () => label.PublishVersion(
            label.Draft!.Id, new DateOnly(2026, 6, 1), Now);

        publish.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(GlobalLabelErrors.ContentRequiredToPublish);
    }

    [Fact]
    public void PublishingPutsTheVersionInForceOnItsOwnDate()
    {
        var label = APublishedLabel(new DateOnly(2026, 6, 1));

        var inForce = label.VersionInForce!;

        inForce.VersionNumber.Should().Be(1);
        inForce.EffectiveFrom.Should().Be(new DateOnly(2026, 6, 1));
        inForce.EffectiveTo.Should().BeNull();

        // The publish and the effect are two different dates, kept apart.
        inForce.PublishedOnUtc.Should().Be(Now);
    }

    /// <summary>
    /// <b>The core invariant.</b> Publishing and superseding are one act, so a
    /// label family can never have two versions in force.
    /// </summary>
    [Fact]
    public void PublishingTheNextVersionRetiresTheOneItReplaces()
    {
        var label = APublishedLabel(new DateOnly(2026, 6, 1));

        var second = label.StartDraft();
        label.AttachContent(second.Id, ProductDocumentId.New());
        label.PublishVersion(second.Id, new DateOnly(2026, 9, 1), Now);

        label.VersionInForce!.VersionNumber.Should().Be(2);

        var retired = label.Versions.Single(x => x.VersionNumber == 1);

        retired.Status.Should().Be(GlobalLabelVersionStatus.Superseded);

        // The ranges meet exactly: no gap, no overlap, and no day on which two
        // versions were both in force.
        retired.EffectiveTo.Should().Be(new DateOnly(2026, 8, 31));
    }

    [Fact]
    public void AVersionCannotTakeEffectBeforeTheOneItReplaces()
    {
        var label = APublishedLabel(new DateOnly(2026, 6, 1));

        var second = label.StartDraft();
        label.AttachContent(second.Id, ProductDocumentId.New());

        var publish = () => label.PublishVersion(
            second.Id, new DateOnly(2026, 5, 1), Now);

        publish.Should().Throw<DomainException>()
            .WithMessage(GlobalLabelErrors.EffectiveFromNotAfterVersionInForce);
    }

    /// <summary>
    /// Same day is refused too — two versions both in force on one date is
    /// exactly the ambiguity the rule exists to prevent.
    /// </summary>
    [Fact]
    public void AVersionCannotTakeEffectOnTheSameDayAsTheOneItReplaces()
    {
        var label = APublishedLabel(new DateOnly(2026, 6, 1));

        var second = label.StartDraft();
        label.AttachContent(second.Id, ProductDocumentId.New());

        var publish = () => label.PublishVersion(
            second.Id, new DateOnly(2026, 6, 1), Now);

        publish.Should().Throw<DomainException>()
            .WithMessage(GlobalLabelErrors.EffectiveFromNotAfterVersionInForce);
    }

    [Fact]
    public void APublishedVersionIsFrozen()
    {
        var label = APublishedLabel(new DateOnly(2026, 6, 1));
        var published = label.VersionInForce!.Id;

        var attach = () => label.AttachContent(published, ProductDocumentId.New());

        attach.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(GlobalLabelErrors.VersionNotDraft);
    }

    [Fact]
    public void VersionNumbersRunOnPastTheHighestEverIssued()
    {
        var label = APublishedLabel(new DateOnly(2026, 6, 1));

        label.StartDraft().VersionNumber.Should().Be(2);
    }

    [Fact]
    public void ADraftCanBeThrownAway()
    {
        var label = APublishedLabel(new DateOnly(2026, 6, 1));

        label.StartDraft();
        label.DiscardDraft();

        label.Draft.Should().BeNull();
        label.Versions.Should().HaveCount(1);

        // The number is reissued, because nothing ever cited the discarded one.
        label.StartDraft().VersionNumber.Should().Be(2);
    }

    /// <summary>
    /// The guard is <c>Draft</c>, not "not in force" — so a superseded issue is
    /// as untouchable as the current one, and the one deletion this aggregate
    /// permits cannot reach a regulatory record.
    /// </summary>
    [Fact]
    public void AVersionThatWasEverInForceCannotBeThrownAway()
    {
        var label = APublishedLabel(new DateOnly(2026, 6, 1));

        var discard = () => label.DiscardDraft();

        discard.Should().Throw<NotFoundException>()
            .WithMessage(GlobalLabelErrors.NoOpenDraft);
    }

    [Fact]
    public void AChangeSummaryBelongsToTheVersionItDescribes()
    {
        var label = ALabel();
        var draft = label.Draft!;

        label.AttachContent(draft.Id, ProductDocumentId.New());
        label.SummariseChanges(draft.Id, "  Paediatric dosing added.  ");
        label.PublishVersion(draft.Id, new DateOnly(2026, 6, 1), Now);

        label.VersionInForce!.ChangeSummary
            .Should().Be("Paediatric dosing added.");
    }

    [Fact]
    public void AVersionFromAnotherLabelIsNotFound()
    {
        var label = ALabel();

        var publish = () => label.PublishVersion(
            GlobalLabelVersionId.New(), new DateOnly(2026, 6, 1), Now);

        publish.Should().Throw<NotFoundException>()
            .WithMessage(GlobalLabelErrors.VersionNotFound);
    }
}
