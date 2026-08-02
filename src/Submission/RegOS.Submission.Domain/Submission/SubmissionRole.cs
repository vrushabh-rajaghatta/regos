using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// A person named on this filing, and what they were named as.
/// </summary>
/// <remarks>
/// <para>
/// <b>Shared vocabulary, separate fact</b> (ADR-048). The <em>names</em> of
/// roles are one thing — <c>ContactRole</c> reference data, the same list
/// <c>Contact.Roles</c> draws on. Who holds a role <em>in general</em> and who
/// was named <em>on this sequence</em> are two different facts about two
/// different subjects, and this records the second.
/// </para>
/// <para>
/// It follows that naming someone as Qualified Person here does <b>not</b>
/// require their contact profile to list Qualified Person. The profile is
/// organisational metadata; this is a historical record of what was declared.
/// If they disagree that is potentially interesting, and it is not invalid.
/// </para>
/// <para>
/// <b>There is deliberately no application-level equivalent.</b> Under the
/// cumulative model (ADR-045) the latest published sequence <em>is</em> the
/// current regulatory state, so an application's current contacts are read from
/// it. A stored copy could only ever differ by being stale — the same argument
/// that removed <c>SubmissionSnapshot</c>.
/// </para>
/// </remarks>
public sealed class SubmissionRole : Entity<SubmissionRoleId>
{
    // EF materialisation only.
    private SubmissionRole()
    {
    }

    // Only the Submission aggregate may name someone.
    internal SubmissionRole(
        SubmissionRoleId id,
        ContactId contactId,
        ContactRoleId roleId)
    {
        Id = id;
        ContactId = contactId;
        RoleId = roleId;
    }

    /// <summary>The person, held by id and never navigated to (ES-014).</summary>
    public ContactId ContactId { get; private set; } = default!;

    /// <summary>What they were named as — a <c>ContactRole</c>.</summary>
    public ContactRoleId RoleId { get; private set; }
}
