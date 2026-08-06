using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Process.Application.Queries.GetProcessPlan;

/// <summary>
/// What the rest of RegOS says it attached to a plan's steps.
/// </summary>
/// <remarks>
/// <b>Six reads, and all six are reads</b> (ADR-065 D2, I2). Process holds no
/// foreign key to any of these aggregates and no repository for any of them —
/// each owns its own <c>ProcessStepId</c> and its own command. This is the whole
/// of what Process is permitted to know about them, and it is deliberately
/// nothing beyond an id and a label.
/// <para>
/// <b>Extracted from the query handler, not abstracted.</b> Six projections that
/// happen to look alike are not yet evidence of one abstraction (ADR-018), and an
/// <c>IAttachmentProvider</c> here would invert the dependency this epic spent
/// six stories keeping pointed inwards. Moving the composition beside the handler
/// is mechanical; giving it a polymorphic seam would not be.
/// </para>
/// <para>
/// <b>Six is the whole surface.</b> ADR-065 authorised three inbound edges —
/// Registration and Submission at S006, Interaction at S007 — and they are now
/// spent. A seventh read means a fourth context took an edge that was never
/// granted, which <c>ContextDependencyTests</c> refuses before this file is
/// reached.
/// </para>
/// </remarks>
internal static class PlanAttachments
{
    /// <summary>
    /// Everything attached to any of <paramref name="stepIds"/>, grouped by step.
    /// One query per source over the whole plan, never one per step.
    /// </summary>
    public static async Task<IReadOnlyDictionary<ProcessStepId, List<AttachedArtefact>>>
        ForStepsAsync(
            RegOSDbContext dbContext,
            IReadOnlyCollection<ProcessStepId> stepIds,
            CancellationToken cancellationToken)
    {
        var found = new List<(ProcessStepId Step, AttachedArtefact Artefact)>();

        // What the plan produced.
        found.AddRange((await dbContext.Submissions
            .AsNoTracking()
            .Where(x => x.ProcessStepId != null && stepIds.Contains(x.ProcessStepId))
            .Select(x => new { Step = x.ProcessStepId!, x.Id, x.Title })
            .ToListAsync(cancellationToken))
            .Select(x => (x.Step, new AttachedArtefact("Submission", x.Id.Value, x.Title))));

        found.AddRange((await dbContext.Registrations
            .AsNoTracking()
            .Where(x => x.ProcessStepId != null && stepIds.Contains(x.ProcessStepId))
            .Select(x => new { Step = x.ProcessStepId!, x.Id, x.RegistrationNumber })
            .ToListAsync(cancellationToken))
            .Select(x => (x.Step, new AttachedArtefact(
                "Registration", x.Id.Value, x.RegistrationNumber ?? "Registration"))));

        // What the plan involved. The pre-IND track of the seeded playbook is
        // three steps whose real artefacts are all in this half — which is why
        // S007 exists and S006 was not enough.
        found.AddRange((await dbContext.HaCorrespondence
            .AsNoTracking()
            .Where(x => x.ProcessStepId != null && stepIds.Contains(x.ProcessStepId))
            .Select(x => new { Step = x.ProcessStepId!, x.Id, x.Subject })
            .ToListAsync(cancellationToken))
            .Select(x => (x.Step, new AttachedArtefact(
                "Correspondence", x.Id.Value, x.Subject))));

        found.AddRange((await dbContext.HaMeetings
            .AsNoTracking()
            .Where(x => x.ProcessStepId != null && stepIds.Contains(x.ProcessStepId))
            .Select(x => new { Step = x.ProcessStepId!, x.Id, x.Subject })
            .ToListAsync(cancellationToken))
            .Select(x => (x.Step, new AttachedArtefact("Meeting", x.Id.Value, x.Subject))));

        found.AddRange((await dbContext.Inspections
            .AsNoTracking()
            .Where(x => x.ProcessStepId != null && stepIds.Contains(x.ProcessStepId))
            .Select(x => new { Step = x.ProcessStepId!, x.Id, x.Title })
            .ToListAsync(cancellationToken))
            .Select(x => (x.Step, new AttachedArtefact("Inspection", x.Id.Value, x.Title))));

        found.AddRange((await dbContext.Commitments
            .AsNoTracking()
            .Where(x => x.ProcessStepId != null && stepIds.Contains(x.ProcessStepId))
            .Select(x => new { Step = x.ProcessStepId!, x.Id, x.Title })
            .ToListAsync(cancellationToken))
            .Select(x => (x.Step, new AttachedArtefact("Commitment", x.Id.Value, x.Title))));

        return found
            .GroupBy(x => x.Step)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Artefact).ToList());
    }
}
