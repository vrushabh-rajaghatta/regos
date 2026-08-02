namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// A submission's own lifecycle — <b>only the states we are the actor of</b>
/// (ADR-046).
/// </summary>
/// <remarks>
/// What the authority does is not here. <c>Acknowledged</c>, <c>UnderReview</c>,
/// <c>Approved</c> and <c>Refused</c> can all change without anything happening
/// to the submission, which is the test that puts them in the regulatory
/// conversation rather than in this enum: they arrive as
/// <c>HaCorrespondence</c> anchored to the submission, and a licence outcome is
/// a <c>Registration</c>, which already carries every one of those words.
/// <para>
/// <c>Withdrawn</c> is absent for a different reason: you cannot un-file a
/// sequence. A later sequence withdraws an earlier one, which makes withdrawal a
/// relationship between submissions rather than a state of either.
/// </para>
/// <para>
/// Internal production states — <em>in preparation</em>, <em>ready to
/// submit</em>, and the QC / publishing / compilation / validation pipelines —
/// describe how a team works, not what was filed. They belong with review and
/// approval (EPIC-008).
/// </para>
/// </remarks>
public enum SubmissionStatus
{
    Draft = 1,

    /// <summary>
    /// Frozen. The document set can no longer change, the sequence number is
    /// claimed, and every placed document carries the operation this filing
    /// performed (ADR-045).
    /// </summary>
    Published = 2,

    /// <summary>
    /// Transmitted to the authority. **Defined and unreachable.**
    /// </summary>
    /// <remarks>
    /// Nothing transitions into this state, and that is deliberate rather than
    /// unfinished. Until EPIC-007 generates the eCTD package and sends it, the
    /// artefact that reaches the authority is assembled outside RegOS — so
    /// marking a RegOS submission "filed" would record that <em>something
    /// related</em> went, which is a fact the system cannot honestly make true.
    /// <para>
    /// The value exists because the state is real and because ADR-044 leaned on
    /// it: that ADR described a null sequence number as "never transmitted",
    /// when publishing only freezes. **Published within RegOS is what a
    /// sequence number means today** (ADR-046 amends ADR-044 on this point);
    /// EPIC-007 adds the transition that makes the stronger word true.
    /// </para>
    /// </remarks>
    Filed = 3
}
