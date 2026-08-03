using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Services;

/// <summary>
/// Whether an application number may still change — <b>a question about
/// submissions, asked from outside the aggregate that holds the number.</b>
/// </summary>
/// <remarks>
/// An application number is correctable in principle: RegOS's record of an
/// external fact can be wrong, and refusing a correction would force someone to
/// delete a regulatory record to fix a typo.
/// <para>
/// <b>It stops being correctable the moment a sequence carries it to the
/// authority.</b> `us-regional.xml` renders it into every published sequence, so
/// changing it afterwards would rewrite what was filed — the reasoning ADR-045
/// and ADR-047 apply to everything else that is frozen at publication.
/// </para>
/// <para>
/// A policy rather than an aggregate rule because `RegulatoryApplication` holds
/// no submissions and must not: aggregates reference each other by id (ES-014).
/// The interface lives here and the query lives in Infrastructure, the same
/// shape as <see cref="IRegulatoryApplicationCreationPolicy"/>.
/// </para>
/// </remarks>
public interface IApplicationNumberPolicy
{
    Task EnsureTheNumberCanStillChangeAsync(
        RegulatoryApplicationAggregate application,
        string proposedNumber,
        CancellationToken cancellationToken);
}
