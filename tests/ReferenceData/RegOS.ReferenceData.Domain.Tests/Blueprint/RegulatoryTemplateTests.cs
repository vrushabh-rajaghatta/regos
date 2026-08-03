using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Blueprint;

public class RegulatoryTemplateTests
{
    private static RegulatoryTemplate NewTemplate() =>
        RegulatoryTemplate.Create(
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "ICH eCTD");

    [Fact]
    public void Create_StartsActive()
    {
        NewTemplate().Status.Should().Be(RegulatoryTemplateStatus.Active);
    }

    [Fact]
    public void Create_NormalizesCode()
    {
        var template = RegulatoryTemplate.Create(
            "  fda_ind_ctd  ",
            "FDA IND (CTD)",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "ICH eCTD");

        template.Code.Should().Be("FDA_IND_CTD");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankCode_Throws(string code)
    {
        var act = () => RegulatoryTemplate.Create(
            code,
            "FDA IND (CTD)",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "ICH eCTD");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.CodeRequired);
    }

    [Fact]
    public void Create_BlankName_Throws()
    {
        var act = () => RegulatoryTemplate.Create(
            "FDA_IND_CTD",
            "  ",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "ICH eCTD");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.NameRequired);
    }

    [Fact]
    public void Create_MissingAuthority_Throws()
    {
        var act = () => RegulatoryTemplate.Create(
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            default,
            new ApplicationTypeId(Guid.NewGuid()),
            "ICH eCTD");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.AuthorityRequired);
    }

    [Fact]
    public void Create_MissingApplicationType_Throws()
    {
        var act = () => RegulatoryTemplate.Create(
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(Guid.NewGuid()),
            default,
            "ICH eCTD");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.ApplicationTypeRequired);
    }

    [Fact]
    public void Create_BlankSource_Throws()
    {
        var act = () => RegulatoryTemplate.Create(
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "  ");

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.SourceRequired);
    }

    [Fact]
    public void StartDraftVersion_FirstIsDraftNumberedOne()
    {
        var template = NewTemplate();

        var version = template.StartDraftVersion();

        version.VersionNumber.Should().Be(1);
        version.Status.Should().Be(TemplateVersionStatus.Draft);
        template.Versions.Should().HaveCount(1);
    }

    [Fact]
    public void StartDraftVersion_WhileDraftOpen_Throws()
    {
        var template = NewTemplate();
        template.StartDraftVersion();

        var act = () => template.StartDraftVersion();

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.DraftAlreadyExists);
    }

    [Fact]
    public void StartDraftVersion_AfterPublish_IsNumberedTwo()
    {
        var template = NewTemplate();
        var v1 = template.StartDraftVersion();
        template.PublishVersion(v1.Id, null, DateTime.UtcNow);

        var v2 = template.StartDraftVersion();

        v2.VersionNumber.Should().Be(2);
    }

    [Fact]
    public void PublishVersion_MovesDraftToPublished()
    {
        var template = NewTemplate();
        var version = template.StartDraftVersion();
        var effectiveFrom = new DateOnly(2026, 1, 1);
        var publishedOn = DateTime.UtcNow;

        template.PublishVersion(version.Id, effectiveFrom, publishedOn);

        version.Status.Should().Be(TemplateVersionStatus.Published);
        version.EffectiveFrom.Should().Be(effectiveFrom);
        version.PublishedOnUtc.Should().Be(publishedOn);
    }

    [Fact]
    public void PublishVersion_UnknownVersion_Throws()
    {
        var template = NewTemplate();

        var act = () => template.PublishVersion(
            RegulatoryTemplateVersionId.New(), null, DateTime.UtcNow);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.VersionNotFound);
    }

    [Fact]
    public void PublishVersion_AlreadyPublished_Throws()
    {
        var template = NewTemplate();
        var version = template.StartDraftVersion();
        template.PublishVersion(version.Id, null, DateTime.UtcNow);

        var act = () => template.PublishVersion(
            version.Id, null, DateTime.UtcNow);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.VersionAlreadyPublished);
    }
}
