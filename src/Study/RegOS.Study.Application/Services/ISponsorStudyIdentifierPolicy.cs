using RegOS.SharedKernel.Primitives;

namespace RegOS.Study.Application.Services;

/// <summary>
/// One sponsor study identifier means one study, across both kinds.
/// </summary>
/// <remarks>
/// <b>The rule comes from outside RegOS.</b> E24 records that FDA's review
/// tooling recognises a study by its <c>study-id</c>, and that a mismatch shows
/// the reviewer two studies where there is one. The converse is the same defect
/// read backwards: two studies sharing an identifier are shown as one, and the
/// STF carries no kind marker to tell them apart — it writes
/// <c>&lt;study-id&gt;ABC-123&lt;/study-id&gt;</c> and nothing else.
/// <para>
/// <b>Which is why it spans both aggregates.</b> A unique index can only cover
/// one table, and uniqueness within each kind would still let a clinical and a
/// nonclinical study collide in the one namespace FDA reads. ADR-056 left the
/// choice open and required that whichever was made got a test; this is the
/// choice, and <c>SponsorStudyIdentifierPolicyTests</c> is the test.
/// </para>
/// <para>
/// A cross-aggregate rule, so it is a policy rather than an invariant — the
/// same shape as <c>IRegistrationCreationPolicy</c>. The unique indexes behind
/// it close the race this cannot; together they are belt and braces, and neither
/// alone states the rule.
/// </para>
/// </remarks>
public interface ISponsorStudyIdentifierPolicy
{
    /// <param name="excluding">
    /// The study already holding this identifier legitimately — its own row, on
    /// a correction. Null when registering.
    /// </param>
    Task EnsureUnusedAsync(
        TenantId tenantId,
        string sponsorStudyIdentifier,
        Guid? excluding,
        CancellationToken cancellationToken);
}
