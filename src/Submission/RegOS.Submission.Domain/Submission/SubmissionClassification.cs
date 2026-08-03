using RegOS.ReferenceData.Domain.SubmissionSubType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// What a submission is filed under: either it starts a new regulatory activity
/// and says what that activity is, or it continues one already opened — and in
/// both cases it says what this particular sequence does.
/// </summary>
/// <remarks>
/// <b>The exclusive-or is unconstructible here rather than checked on the
/// aggregate.</b> There are two ways to build one and neither can produce both
/// facts, so <i>"a continuing sequence must not carry its own activity type"</i>
/// is not a rule anybody can break — it is a shape.
/// <para>
/// That matters because the two would be a copy of one fact, and two copies can
/// only ever differ by one of them being wrong. The same reasoning chose
/// <see cref="OriginatingSubmission.IsItselfAnOrigin"/> over transitive
/// resolution.
/// </para>
/// <para>
/// <b>The database still carries the rule as a CHECK constraint</b>, and that is
/// not redundancy: this type governs code, the constraint governs data, and a
/// migration or a manual UPDATE never passes through here (the division of
/// labour of ADR-044 decision 6).
/// </para>
/// </remarks>
public sealed record SubmissionClassification
{
    private SubmissionClassification(
        SubmissionTypeId? submissionTypeId,
        OriginatingSubmission? origin,
        SubmissionSubTypeId submissionSubTypeId)
    {
        SubmissionTypeId = submissionTypeId;
        Origin = origin;
        SubmissionSubTypeId = submissionSubTypeId;
    }

    /// <summary>What the activity is. Set only when this submission opens it.</summary>
    public SubmissionTypeId? SubmissionTypeId { get; }

    /// <summary>The sequence that opened the activity. Set only when continuing.</summary>
    public OriginatingSubmission? Origin { get; }

    /// <summary>
    /// What this sequence does to the activity. Required either way, and
    /// <b>never inferred</b> — FDA example #23 is an opening sequence whose
    /// sub-type is <c>report</c> (evidence E13).
    /// </summary>
    public SubmissionSubTypeId SubmissionSubTypeId { get; }

    /// <summary>
    /// This submission starts a new regulatory activity of the given type.
    /// </summary>
    public static SubmissionClassification Opens(
        SubmissionTypeId submissionTypeId,
        SubmissionSubTypeId submissionSubTypeId)
    {
        if (submissionTypeId == default)
            throw new DomainException(SubmissionErrors.SubmissionTypeRequired);

        if (submissionSubTypeId == default)
            throw new DomainException(SubmissionErrors.SubmissionSubTypeRequired);

        return new SubmissionClassification(
            submissionTypeId, null, submissionSubTypeId);
    }

    /// <summary>
    /// This submission continues the activity that <paramref name="origin"/>
    /// opened. It carries no type of its own — the activity's type is the
    /// origin's, and asking for it again would be asking for a second copy.
    /// </summary>
    public static SubmissionClassification Continues(
        OriginatingSubmission origin,
        SubmissionSubTypeId submissionSubTypeId)
    {
        ArgumentNullException.ThrowIfNull(origin);

        if (submissionSubTypeId == default)
            throw new DomainException(SubmissionErrors.SubmissionSubTypeRequired);

        return new SubmissionClassification(null, origin, submissionSubTypeId);
    }
}
