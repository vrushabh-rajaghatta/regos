using FluentAssertions;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Domain.Tests;

/// <summary>
/// The playbook's own rules — numbering, the single open draft, and the
/// one-way door that
/// <see href="../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md">ADR-065</see>
/// I4 turns on.
/// </summary>
public class ProcessDefinitionTests
{
    [Fact]
    public void A_new_playbook_is_active_and_has_no_versions()
    {
        var definition = ADefinition();

        definition.Status.Should().Be(ProcessDefinitionStatus.Active);
        definition.Versions.Should().BeEmpty();
        definition.Draft.Should().BeNull();
        definition.CurrentVersion.Should().BeNull();
    }

    [Fact]
    public void The_playbook_numbers_its_own_versions()
    {
        var definition = ADefinition();

        var first = definition.StartDraftVersion();
        definition.AddStep("A", "First");
        definition.PublishVersion(first.Id, null, DateTime.UtcNow);

        var second = definition.StartDraftVersion();

        first.VersionNumber.Should().Be(1);
        second.VersionNumber.Should().Be(2);
    }

    [Fact]
    public void Only_one_draft_may_be_open_at_a_time()
    {
        var definition = ADefinition();
        definition.StartDraftVersion();

        var second = () => definition.StartDraftVersion();

        second.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.DraftAlreadyOpen);
    }

    /// <summary>
    /// <b>The invariant the whole context rests on.</b> A plan may already be
    /// pinned to this version, so its steps can never change again.
    /// </summary>
    [Fact]
    public void A_published_version_can_never_be_edited_again()
    {
        var definition = ADefinition();
        var version = definition.StartDraftVersion();
        definition.AddStep("A", "First");
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        var edit = () => definition.AddStep("B", "Second");

        edit.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.NoOpenDraft);
    }

    [Fact]
    public void A_version_cannot_be_published_twice()
    {
        var definition = ADefinition();
        var version = definition.StartDraftVersion();
        definition.AddStep("A", "First");
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        var again = () => definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.VersionAlreadyPublished);
    }

    /// <summary>
    /// A playbook with nothing in it would instantiate an empty plan, which is a
    /// worse outcome than refusing to publish.
    /// </summary>
    [Fact]
    public void An_empty_version_cannot_be_published()
    {
        var definition = ADefinition();
        var version = definition.StartDraftVersion();

        var publish = () => definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        publish.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.NoSteps);
    }

    [Fact]
    public void Superseding_leaves_the_version_readable_and_out_of_selection()
    {
        var definition = ADefinition();

        var first = definition.StartDraftVersion();
        definition.AddStep("A", "First");
        definition.PublishVersion(first.Id, null, DateTime.UtcNow);

        var second = definition.StartDraftVersion();
        definition.AddStep("A", "First, restated");
        definition.PublishVersion(second.Id, null, DateTime.UtcNow);

        definition.SupersedeVersion(first.Id);

        first.Status.Should().Be(ProcessDefinitionVersionStatus.Superseded);
        first.Steps.Should().HaveCount(1, "a superseded version keeps its steps — "
            + "plans pinned to it are still scheduled from them");
        definition.CurrentVersion.Should().Be(second);
    }

    [Fact]
    public void A_draft_is_discarded_rather_than_superseded()
    {
        var definition = ADefinition();
        var draft = definition.StartDraftVersion();

        var supersede = () => definition.SupersedeVersion(draft.Id);

        supersede.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProcessDefinitionErrors.OnlyPublishedVersionsCanBeSuperseded);
    }

    /// <summary>
    /// A discarded draft's number is reissued; a number some plan was scheduled
    /// from never is.
    /// </summary>
    [Fact]
    public void Discarding_a_draft_reissues_its_number()
    {
        var definition = ADefinition();

        definition.StartDraftVersion();
        definition.DiscardDraft();

        definition.StartDraftVersion().VersionNumber.Should().Be(1);
    }

    [Fact]
    public void Retiring_a_playbook_keeps_every_version_it_published()
    {
        var definition = ADefinition();
        var version = definition.StartDraftVersion();
        definition.AddStep("A", "First");
        definition.PublishVersion(version.Id, null, DateTime.UtcNow);

        definition.Retire();

        definition.Status.Should().Be(ProcessDefinitionStatus.Retired);
        definition.Versions.Should().HaveCount(1);
        definition.CurrentVersion.Should().Be(version);
    }

    /// <summary>
    /// The scope is not optional, and <c>default</c> is what an unset one looks
    /// like: <c>CountryId</c> and its siblings are flat master data and stay
    /// record structs permanently (ADR-043 §2), so there is no null to check.
    /// </summary>
    [Fact]
    public void A_playbook_needs_a_country_an_authority_and_an_application_type()
    {
        var create = () => ProcessDefinition.Create(
            "CODE",
            "Name",
            default,
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            DateTime.UtcNow);

        create.Should().Throw<DomainException>()
            .WithMessage(ProcessDefinitionErrors.CountryRequired);
    }

    public static ProcessDefinition ADefinition()
        => ProcessDefinition.Create(
            "US-FDA-IND-INITIAL",
            "US FDA IND — initial filing",
            new CountryId(Guid.NewGuid()),
            new AuthorityId(Guid.NewGuid()),
            new ApplicationTypeId(Guid.NewGuid()),
            DateTime.UtcNow);
}
