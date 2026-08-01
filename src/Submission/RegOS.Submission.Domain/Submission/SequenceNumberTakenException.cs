using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// Another submission in the same application was published under this sequence
/// number first. Raised by the persistence layer when the unique index on
/// (application, sequence number) rejects the write.
/// </summary>
/// <remarks>
/// <b>Normally invisible.</b> Publishing retries on this — a fresh number is
/// read and the write is attempted again — so it reaches a caller only when the
/// retry budget is exhausted under sustained contention on one application.
/// <para>
/// A <see cref="BusinessRuleViolationException"/> deliberately: the request was
/// well formed and would have succeeded against a different system state, which
/// is exactly 409 (ADR-012). No new middleware branch, and a caller that does
/// see it is being told the right thing — try again.
/// </para>
/// <para>
/// The index is the authority on uniqueness, not this exception and not the
/// numbering policy (ADR-044 decision 6). A race that the policy cannot see is
/// precisely what it exists to convert into something the application can act on.
/// </para>
/// </remarks>
public sealed class SequenceNumberTakenException : BusinessRuleViolationException
{
    public SequenceNumberTakenException()
        : base("Another submission in this application was published under that "
               + "sequence number. Please try again.")
    {
    }
}
