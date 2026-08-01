using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Interaction.Domain.Correspondence;

/// <summary>
/// A letter, email or formal communication between the sponsor and a health
/// authority, in either direction.
/// </summary>
/// <remarks>
/// <b>It is an event, not a lifecycle, and therefore has no status.</b> Every
/// other object in this context evolves — a question is answered, a commitment
/// is fulfilled, a meeting is held. A letter that has been received does not
/// change; what changes is our response to it. Whether a piece of correspondence
/// is "open" is derived from its <see cref="ResponseDueOn"/> and, from S003, its
/// unresolved questions. Persist the fact, derive the interpretation (ADR-037).
/// <para>
/// <b>All three anchors are nullable, deliberately.</b> Most correspondence
/// concerns an application, a submission or a registration — but a guidance
/// notification or a general enquiry concerns none of them, and an interaction
/// that cannot be filed against anything is still a real interaction. Requiring
/// an anchor would push users to invent one.
/// </para>
/// <para>
/// <b>There is no division or contact here yet, and its absence is a finding.</b>
/// The division that sends a letter is the <em>authority's</em>, and
/// <c>OrganizationDivision</c> hangs off a tenant-owned <c>Organization</c>
/// whose types are all commercial — there is no way to express "FDA" as one.
/// Widening that enum would create a second FDA that can disagree with the
/// reference-data one, which ADR-039 decision 1 forbids. EPIC-006 S001a
/// introduces authority-side structure under <see cref="Authority"/>; until
/// then this carries no field rather than a misleading one, and ADR-038's
/// prediction that EPIC-006 would hold an <c>OrganizationDivisionId</c> is
/// recorded as falsified.
/// </para>
/// </remarks>
public sealed class HaCorrespondence : AggregateRoot<HaCorrespondenceId>
{
    public const int SubjectMaxLength = 300;
    public const int ReferenceMaxLength = 100;

    // Parameterized private constructor and no parameterless one — EF binds by
    // parameter name, which keeps every non-optional field non-nullable.
    private HaCorrespondence(
        HaCorrespondenceId id,
        TenantId tenantId,
        AuthorityId authorityId,
        CorrespondenceTypeId correspondenceTypeId,
        CorrespondenceDirection direction,
        string subject,
        DateOnly occurredOn,
        DateOnly? responseDueOn,
        string? authorityReference,
        RegulatoryApplicationId? regulatoryApplicationId,
        SubmissionId? submissionId,
        RegistrationId? registrationId,
        DateTime recordedOnUtc)
    {
        Id = id;
        TenantId = tenantId;
        AuthorityId = authorityId;
        CorrespondenceTypeId = correspondenceTypeId;
        Direction = direction;
        Subject = subject;
        OccurredOn = occurredOn;
        ResponseDueOn = responseDueOn;
        AuthorityReference = authorityReference;
        RegulatoryApplicationId = regulatoryApplicationId;
        SubmissionId = submissionId;
        RegistrationId = registrationId;
        RecordedOnUtc = recordedOnUtc;
    }

    /// <summary>The owning tenant (ADR-031). Set once.</summary>
    public TenantId TenantId { get; } = default!;

    /// <summary>The authority we corresponded with. Immutable.</summary>
    public AuthorityId AuthorityId { get; }

    public CorrespondenceTypeId CorrespondenceTypeId { get; private set; }

    public CorrespondenceDirection Direction { get; }

    public string Subject { get; private set; } = default!;

    /// <summary>
    /// The date on the letter — the business date, not when RegOS learned of
    /// it. Paired with <see cref="RecordedOnUtc"/> for the same reason the
    /// status histories separate the two: a letter logged today may be from
    /// 2019, and both facts are true.
    /// </summary>
    public DateOnly OccurredOn { get; private set; }

    /// <summary>
    /// When a response is due, if one is. Who owes it follows from
    /// <see cref="Direction"/> and is never stored.
    /// </summary>
    public DateOnly? ResponseDueOn { get; private set; }

    /// <summary>The authority's own reference or docket number, as printed.</summary>
    public string? AuthorityReference { get; private set; }

    public RegulatoryApplicationId? RegulatoryApplicationId { get; private set; }

    public SubmissionId? SubmissionId { get; private set; }

    public RegistrationId? RegistrationId { get; private set; }

    /// <summary>When RegOS learned of it.</summary>
    public DateTime RecordedOnUtc { get; }

    public static HaCorrespondence Record(
        TenantId tenantId,
        AuthorityId authorityId,
        CorrespondenceTypeId correspondenceTypeId,
        CorrespondenceDirection direction,
        string subject,
        DateOnly occurredOn,
        DateOnly? responseDueOn = null,
        string? authorityReference = null,
        RegulatoryApplicationId? regulatoryApplicationId = null,
        SubmissionId? submissionId = null,
        RegistrationId? registrationId = null)
    {
        if (tenantId is null)
            throw new DomainException(HaCorrespondenceErrors.TenantRequired);

        if (authorityId == default)
            throw new DomainException(HaCorrespondenceErrors.AuthorityRequired);

        if (correspondenceTypeId == default)
            throw new DomainException(HaCorrespondenceErrors.CorrespondenceTypeRequired);

        var trimmedSubject = Validated(subject);
        var trimmedReference = ValidatedReference(authorityReference);

        if (responseDueOn is { } due && due < occurredOn)
            throw new DomainException(HaCorrespondenceErrors.ResponseDueBeforeOccurred);

        return new HaCorrespondence(
            HaCorrespondenceId.New(),
            tenantId,
            authorityId,
            correspondenceTypeId,
            direction,
            trimmedSubject,
            occurredOn,
            responseDueOn,
            trimmedReference,
            regulatoryApplicationId,
            submissionId,
            registrationId,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Corrects what was typed. A letter's own facts — its authority, its
    /// direction, the date printed on it — are not editable here: getting those
    /// wrong means the wrong letter was logged, and the record should be
    /// superseded rather than rewritten.
    /// </summary>
    public void Amend(
        CorrespondenceTypeId correspondenceTypeId,
        string subject,
        DateOnly occurredOn,
        DateOnly? responseDueOn,
        string? authorityReference)
    {
        if (correspondenceTypeId == default)
            throw new DomainException(HaCorrespondenceErrors.CorrespondenceTypeRequired);

        var trimmedSubject = Validated(subject);
        var trimmedReference = ValidatedReference(authorityReference);

        if (responseDueOn is { } due && due < occurredOn)
            throw new DomainException(HaCorrespondenceErrors.ResponseDueBeforeOccurred);

        CorrespondenceTypeId = correspondenceTypeId;
        Subject = trimmedSubject;
        OccurredOn = occurredOn;
        ResponseDueOn = responseDueOn;
        AuthorityReference = trimmedReference;
    }

    /// <summary>
    /// Files the correspondence against what it concerns, or against nothing.
    /// All three are set together because they answer one question — <em>what is
    /// this about?</em> — and setting them individually would let a letter
    /// accumulate anchors nobody chose.
    /// </summary>
    public void FileAgainst(
        RegulatoryApplicationId? regulatoryApplicationId,
        SubmissionId? submissionId,
        RegistrationId? registrationId)
    {
        RegulatoryApplicationId = regulatoryApplicationId;
        SubmissionId = submissionId;
        RegistrationId = registrationId;
    }

    private static string Validated(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException(HaCorrespondenceErrors.SubjectRequired);

        var trimmed = subject.Trim();

        if (trimmed.Length > SubjectMaxLength)
            throw new DomainException(HaCorrespondenceErrors.SubjectTooLong);

        return trimmed;
    }

    private static string? ValidatedReference(string? authorityReference)
    {
        if (string.IsNullOrWhiteSpace(authorityReference))
            return null;

        var trimmed = authorityReference.Trim();

        if (trimmed.Length > ReferenceMaxLength)
            throw new DomainException(HaCorrespondenceErrors.ReferenceTooLong);

        return trimmed;
    }
}
