using FluentAssertions;

using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Blueprint;

/// <summary>
/// <b>EPIC-007a S004 — where a section's documents are written on disk.</b>
///
/// The column ships empty on purpose: ICH Appendix 4 carries the directory
/// table and is not in this repository, and a leaf path cannot be derived from
/// a section code without inventing a convention. <b>What is tested here is the
/// shape and the guard, not the values</b> — because the shape is what has been
/// established.
/// </summary>
public class TemplateSectionEctdFolderTests
{
    private static RegulatoryTemplate NewDraftTemplate()
    {
        var template = RegulatoryTemplate.Create(
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            "ICH eCTD");

        template.StartDraftVersion();

        return template;
    }

    /// <summary>
    /// The state every seeded row is in today, and it must be reachable without
    /// ceremony — a blueprint whose placement is unknown is still a blueprint.
    /// </summary>
    [Fact]
    public void ASectionWithNoFolder_IsLegalAndStaysNull()
    {
        var section = NewDraftTemplate().AddSection("3.2.S", "Drug Substance");

        section.EctdFolder.Should().BeNull();
    }

    /// <summary>
    /// Appendix 4 gives sections 2.7.1 to 2.7.6 a file row and no directory
    /// row — their documents go in 2.7's folder. <b>That is a known placement,
    /// not a missing one</b>, and collapsing it into null would make two-thirds
    /// of Module 2 unrenderable for no reason but a convenience.
    /// </summary>
    [Fact]
    public void ASectionThatAddsNoDirectory_IsKnown_NotMissing()
    {
        var section = NewDraftTemplate()
            .AddSection("2.7.4", "Summary of Clinical Safety", ectdFolder: "");

        section.EctdFolder.Should().BeEmpty();
        section.HasEctdPlacement.Should().BeTrue();
    }

    [Fact]
    public void OnlySilenceMeansNotInEvidence()
    {
        var section = NewDraftTemplate().AddSection("3.2.S", "Drug Substance");

        section.EctdFolder.Should().BeNull();
        section.HasEctdPlacement.Should().BeFalse();
    }

    [Fact]
    public void AFolderIsKeptAsGiven()
    {
        var section = NewDraftTemplate()
            .AddSection("3.2.S", "Drug Substance",
                ectdFolder: "32s-drug-substance");

        section.EctdFolder.Should().Be("32s-drug-substance");
    }

    /// <summary>
    /// One section, two directories — FDA's Module 1 root is <c>m1/us</c>, and
    /// the regional level has no section of its own to carry it.
    /// </summary>
    [Fact]
    public void AFolderMayChainSegments()
    {
        var section = NewDraftTemplate()
            .AddSection("M1", "Administrative Information", ectdFolder: "m1/us");

        section.EctdFolder.Should().Be("m1/us");
    }

    /// <summary>
    /// ICH Appendix 2, enforced where the value is created rather than trusted
    /// to the seed: this string becomes a filename, and an illegal one is a
    /// package a regulator's tooling rejects.
    /// </summary>
    [Theory]
    [InlineData("m1/US")]                  // uppercase
    [InlineData("m1 us")]                  // space
    [InlineData("m1.us")]                  // dot
    [InlineData("m1_us")]                  // underscore
    [InlineData("m1//us")]                 // empty segment
    public void AnIllegalFolderName_IsRefused(string folder)
    {
        var act = () => NewDraftTemplate()
            .AddSection("M1", "Administrative Information", ectdFolder: folder);

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.SectionEctdFolderNotLegal);
    }

    [Fact]
    public void ASegmentLongerThanAppendix2Allows_IsRefused()
    {
        var tooLong = new string('a', TemplateSection.MaxFolderSegmentLength + 1);

        var act = () => NewDraftTemplate()
            .AddSection("M1", "Administrative Information", ectdFolder: tooLong);

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryTemplateErrors.SectionEctdFolderNotLegal);
    }

    [Fact]
    public void ASegmentAtExactlyTheLimit_IsAccepted()
    {
        var atLimit = new string('a', TemplateSection.MaxFolderSegmentLength);

        var section = NewDraftTemplate()
            .AddSection("M1", "Administrative Information", ectdFolder: atLimit);

        section.EctdFolder.Should().Be(atLimit);
    }

    /// <summary>
    /// The consequence that makes Appendix 4 a versioning event rather than a
    /// data patch: there is no way to set a folder on a published version,
    /// because there is no way to add a section to one (EPIC-007a S002).
    /// </summary>
    [Fact]
    public void APublishedVersion_CannotAcquireFolders()
    {
        var template = NewDraftTemplate();

        template.PublishVersion(
            template.Versions.Single().Id,
            effectiveFrom: null,
            publishedOnUtc: DateTime.UtcNow);

        var act = () => template.AddSection(
            "M1", "Administrative Information", ectdFolder: "m1/us");

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegulatoryTemplateErrors.NoDraftVersion);
    }
}
