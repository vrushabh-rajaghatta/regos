using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Domain.Tests.Submission;

/// <summary>
/// Who is named on a filing, and as what (ADR-048).
/// </summary>
public class SubmissionRoleTests
{
    private static readonly DateTimeOffset PublishedAt =
        new(2026, 8, 2, 9, 0, 0, TimeSpan.Zero);

    private static readonly ContactRoleId QualifiedPerson =
        new(Guid.Parse("dddddddd-0000-0000-0000-000000000001"));
    private static readonly ContactRoleId RegulatoryContact =
        new(Guid.Parse("dddddddd-0000-0000-0000-000000000002"));

    [Fact]
    public void ADraft_NamesNobodyToBeginWith()
    {
        NewDraft().Roles.Should().BeEmpty();
    }

    [Fact]
    public void AssignRole_NamesThePerson()
    {
        var submission = NewDraft();
        var contact = Contact(1);

        var role = submission.AssignRole(contact, QualifiedPerson);

        role.ContactId.Should().Be(contact);
        role.RoleId.Should().Be(QualifiedPerson);
        submission.Roles.Should().ContainSingle().Which.Should().Be(role);
    }

    /// <summary>
    /// One person may hold several roles on one filing, and two people may share
    /// a role. Neither is unusual — only the exact pair repeats meaninglessly.
    /// </summary>
    [Fact]
    public void OnePerson_MayHoldSeveralRoles()
    {
        var submission = NewDraft();
        var contact = Contact(1);

        submission.AssignRole(contact, QualifiedPerson);
        submission.AssignRole(contact, RegulatoryContact);

        submission.Roles.Should().HaveCount(2);
    }

    [Fact]
    public void TwoPeople_MayShareARole()
    {
        var submission = NewDraft();

        submission.AssignRole(Contact(1), RegulatoryContact);
        submission.AssignRole(Contact(2), RegulatoryContact);

        submission.Roles.Should().HaveCount(2);
    }

    [Fact]
    public void TheSamePersonInTheSameRole_IsRefused()
    {
        var submission = NewDraft();
        var contact = Contact(1);
        submission.AssignRole(contact, QualifiedPerson);

        var act = () => submission.AssignRole(contact, QualifiedPerson);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.ContactAlreadyNamedInThatRole);
    }

    [Fact]
    public void RemoveRole_TakesThePersonOffADraft()
    {
        var submission = NewDraft();
        var role = submission.AssignRole(Contact(1), QualifiedPerson);

        submission.RemoveRole(role.Id);

        submission.Roles.Should().BeEmpty();
    }

    [Fact]
    public void RemovingANamingThatIsNotThere_IsNotFound()
    {
        var submission = NewDraft();

        var act = () => submission.RemoveRole(SubmissionRoleId.New());

        act.Should().Throw<NotFoundException>()
            .WithMessage(SubmissionErrors.RoleNotOnSubmission);
    }

    // --- the freeze ----------------------------------------------------------

    /// <summary>
    /// <b>Who was named on sequence 0003 is a fact about a filing already made</b>
    /// (ADR-048 decision 3). The draft guard is the whole mechanism — the same
    /// call <c>ChangeFormat</c> makes.
    /// </summary>
    [Fact]
    public void NamingSomeone_IsRefusedOncePublished()
    {
        var submission = NewDraft();
        submission.Publish(0, null, [], PublishedAt);

        var act = () => submission.AssignRole(Contact(1), QualifiedPerson);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.RolesLockedUnlessDraft);
    }

    [Fact]
    public void RemovingANaming_IsRefusedOncePublished()
    {
        var submission = NewDraft();
        var role = submission.AssignRole(Contact(1), QualifiedPerson);
        submission.Publish(0, null, [], PublishedAt);

        var act = () => submission.RemoveRole(role.Id);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.RolesLockedUnlessDraft);

        // And the naming is still there — a refused change changes nothing.
        submission.Roles.Should().ContainSingle();
    }

    /// <summary>
    /// Publishing does not require anyone to be named. A sequence that names
    /// nobody is unusual, not invalid, and inventing that rule is exactly what
    /// this epic has declined to do four times.
    /// </summary>
    [Fact]
    public void ASubmissionThatNamesNobody_StillPublishes()
    {
        var submission = NewDraft();

        var act = () => submission.Publish(0, null, [], PublishedAt);

        act.Should().NotThrow();
    }

    [Fact]
    public void AssignRole_RefusesAMissingRole()
    {
        var submission = NewDraft();

        var act = () => submission.AssignRole(Contact(1), default);

        act.Should().Throw<DomainException>()
            .WithMessage(SubmissionErrors.ContactRoleRequired);
    }

    private static ContactId Contact(int n) =>
        new(Guid.Parse($"eeeeeeee-0000-0000-0000-{n:D12}"));

    private static SubmissionAggregate NewDraft() =>
        SubmissionAggregate.Create(
            TenantId.New(),
            new RegulatoryApplicationId(Guid.NewGuid()),
            "Original IND",
            SubmissionFormat.Ectd);
}
