using FluentAssertions;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Blueprint;

/// <summary>
/// EPIC-007a S002 — superseding a published blueprint version.
/// </summary>
/// <remarks>
/// The story that produced this was a factual defect in a published blueprint:
/// FDA's 1.13 is the Annual Report, and RegOS had the Investigator's Brochure
/// there. The correction could not edit the version — a published version is
/// frozen — so it had to be a new one, and the old one had to stop attracting
/// new work without disturbing the work already bound to it.
/// </remarks>
public class TemplateVersionDeprecationTests
{
    private static RegulatoryTemplate NewTemplate() =>
        RegulatoryTemplate.Create(
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "ICH eCTD");

    private static RegulatoryTemplateVersion PublishedVersion(
        RegulatoryTemplate template)
    {
        var version = template.StartDraftVersion();
        template.AddSection("M1", "Administrative Information", null, 1);
        template.PublishVersion(version.Id, null, DateTime.UtcNow);

        return version;
    }

    [Fact]
    public void Deprecate_MovesAPublishedVersionOutOfService()
    {
        var template = NewTemplate();
        var v1 = PublishedVersion(template);

        template.DeprecateVersion(v1.Id);

        v1.Status.Should().Be(TemplateVersionStatus.Deprecated);
    }

    [Fact]
    public void Deprecate_LeavesTheStructureIntact()
    {
        var template = NewTemplate();
        var v1 = PublishedVersion(template);
        var sectionsBefore = v1.Sections.Count;

        template.DeprecateVersion(v1.Id);

        // The whole point: submissions bound to this version were judged
        // against these sections, and they must keep working.
        v1.Sections.Should().HaveCount(sectionsBefore);
    }

    [Fact]
    public void Deprecate_DoesNotReopenItForEditing()
    {
        var template = NewTemplate();
        var v1 = PublishedVersion(template);
        template.DeprecateVersion(v1.Id);

        // Out of service is not the same as editable. There is no draft, so
        // AddSection has nothing to add to.
        var act = () => template.AddSection("1.13", "Annual Report", null, 2);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.NoDraftVersion);
    }

    [Fact]
    public void Deprecate_ADraft_IsRejected()
    {
        var template = NewTemplate();
        var draft = template.StartDraftVersion();

        // A draft nobody should use is discarded. Deprecation says something
        // about a version that was in force.
        var act = () => template.DeprecateVersion(draft.Id);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(
                RegulatoryTemplateErrors.OnlyPublishedVersionsCanBeDeprecated);
    }

    [Fact]
    public void Deprecate_Twice_IsRejected()
    {
        var template = NewTemplate();
        var v1 = PublishedVersion(template);
        template.DeprecateVersion(v1.Id);

        var act = () => template.DeprecateVersion(v1.Id);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.VersionAlreadyDeprecated);
    }

    [Fact]
    public void Deprecate_AnUnknownVersion_IsRejected()
    {
        var template = NewTemplate();
        PublishedVersion(template);

        var act = () => template.DeprecateVersion(RegulatoryTemplateVersionId.New());

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.VersionNotFound);
    }

    [Fact]
    public void ASupersededBlueprint_KeepsBothVersions()
    {
        var template = NewTemplate();
        var v1 = PublishedVersion(template);

        var v2 = template.StartDraftVersion();
        template.AddSection("M1", "Administrative Information", null, 1);
        template.PublishVersion(v2.Id, null, DateTime.UtcNow);
        template.DeprecateVersion(v1.Id);

        // The old version is retained, not removed (ES-018) — a filing made
        // against it must stay explicable.
        template.Versions.Should().HaveCount(2);
        template.Versions.Should().ContainSingle(
            v => v.Status == TemplateVersionStatus.Published
                && v.VersionNumber == 2);
        template.Versions.Should().ContainSingle(
            v => v.Status == TemplateVersionStatus.Deprecated
                && v.VersionNumber == 1);
    }
}
