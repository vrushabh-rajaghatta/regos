namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

/// <summary>
/// <b>What today's facts imply, if nothing changes.</b>
/// </summary>
/// <remarks>
/// <b>Deliberately not a method on <see cref="ProcessPlan"/>.</b> It is an
/// observation <em>about</em> a plan, never a behaviour <em>of</em> one — and a
/// static class that takes the plan and returns a value says so more loudly than
/// a method that could quietly start writing.
/// <para>
/// <b>[ADR-065](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// I7 and I8.</b> Nothing here mutates the plan, nothing is persisted, and
/// nothing proposes what to move. It answers <em>"if nothing changes…"</em> and
/// only that: the moment it suggests a repair it has become a scheduler, which
/// is a different bounded context.
/// </para>
/// <para>
/// <b>The projection may recalculate where the plan may not.</b> Storing a
/// recalculated schedule would let a later change move a milestone that has
/// already been agreed (I4, D5). Deriving what today implies breaks neither: the
/// plan's own dates never change, and the answer is labelled a projection
/// everywhere it surfaces. <em>The forecast is not the ledger.</em>
/// </para>
/// </remarks>
public static class PlanImpact
{
    /// <summary>
    /// Every step downstream of one, transitively.
    /// </summary>
    /// <remarks>
    /// <b>The traversal follows dependency, never execution.</b> A skipped step
    /// says <em>we did not perform this</em>; it does not say the steps after it
    /// stopped depending on what came before. Topology is topology, so a settled
    /// step is walked <em>through</em> — it simply appears as affected rather
    /// than actionable.
    /// </remarks>
    public static IReadOnlyCollection<ProcessStepId> Downstream(
        ProcessPlan plan, ProcessStepId stepId)
        => Downstream(Adapt(plan), stepId);

    /// <inheritdoc cref="Downstream(ProcessPlan, ProcessStepId)"/>
    public static IReadOnlyCollection<ProcessStepId> Downstream(
        IReadOnlyCollection<ScheduledStep> steps, ProcessStepId stepId)
    {
        // Successors are the reverse of the stored predecessor edges — the graph
        // is written pointing backwards so that adding a step touches only the
        // new step, and read forwards here.
        var successors = steps
            .SelectMany(step => step.Predecessors.Select(
                predecessor => (From: predecessor, To: step.Id)))
            .GroupBy(edge => edge.From)
            .ToDictionary(
                group => group.Key,
                group => group.Select(edge => edge.To).ToList());

        var affected = new HashSet<ProcessStepId>();
        var pending = new Stack<ProcessStepId>([stepId]);

        while (pending.Count > 0)
        {
            var current = pending.Pop();

            if (!successors.TryGetValue(current, out var next)) continue;

            foreach (var successor in next.Where(affected.Add))
                pending.Push(successor);
        }

        return affected;
    }

    /// <summary>
    /// Re-derives every date from what has actually happened, and reports how far
    /// the plan's finish has moved.
    /// </summary>
    /// <remarks>
    /// <code>
    /// settled     → its actual dates are authoritative
    /// in progress → starts when it started; cannot end before today
    /// otherwise   → starts no earlier than today, and no earlier than
    ///               (latest predecessor's projected end + 1 + its own offset)
    /// </code>
    /// <b>Each step's offset and duration are recovered from the plan itself</b>
    /// rather than read back from the playbook: the plan carries its own graph and
    /// its own dates, so a projection never touches the definition it came from.
    /// <para>
    /// <b>Slack falls out; it is not special-cased.</b> A step nine days late with
    /// thirty days of slack moves the finish by nothing, because its successors'
    /// projected starts are still governed by a different, later strand. That case
    /// is not hypothetical — EPIC-020 S003 found the FDA IND critical path runs
    /// through the meeting track and not the 150-day CMC package, so the naive
    /// answer would have been wrong by 33 days on the one playbook RegOS ships.
    /// </para>
    /// </remarks>
    public static PlanProjection Project(ProcessPlan plan, DateOnly asOf)
        => Project(Adapt(plan), plan.AnchorDate, asOf);

    /// <inheritdoc cref="Project(ProcessPlan, DateOnly)"/>
    public static PlanProjection Project(
        IReadOnlyCollection<ScheduledStep> all, DateOnly anchorDate, DateOnly asOf)
    {
        // Ordered so the walk is deterministic whatever order EF materialised
        // the collection in — I5's reasoning, applied to a read.
        // Deterministic: a step code is unique within a plan (unique index on
        // (ProcessPlanId, Code)), so this ordering is already total.
        var steps = all
            .OrderBy(step => step.Code, StringComparer.Ordinal)
            .ToList();

        var byId = steps.ToDictionary(step => step.Id);
        var projected = new Dictionary<ProcessStepId, ProjectedStep>();
        var outstanding = steps.ToDictionary(step => step.Id);

        while (outstanding.Count > 0)
        {
            var ready = steps
                .Where(step => outstanding.ContainsKey(step.Id))
                .Where(step => step.Predecessors.All(projected.ContainsKey))
                .ToList();

            // Publication certified the graph is acyclic and instantiation copied
            // it unchanged, so this cannot happen. If it does, something upstream
            // broke a guarantee rather than the caller sending bad input.
            if (ready.Count == 0)
                throw new InvalidOperationException(
                    ProcessPlanErrors.CertificateBroken
                    + string.Join(", ", outstanding.Values.Select(x => x.Code)));

            foreach (var step in ready)
            {
                projected[step.Id] = ProjectOne(step, anchorDate, asOf, byId, projected);
                outstanding.Remove(step.Id);
            }
        }

        var plannedFinish = steps.Count == 0
            ? (DateOnly?)null
            : steps.Max(step => step.PlannedEndOn);

        var projectedFinish = projected.Count == 0
            ? (DateOnly?)null
            : projected.Values.Max(step => step.ProjectedEndOn);

        var slip = plannedFinish is { } planned && projectedFinish is { } actual
            ? Math.Max(0, actual.DayNumber - planned.DayNumber)
            : 0;

        return new PlanProjection(plannedFinish, projectedFinish, slip, projected);
    }

    private static ProjectedStep ProjectOne(
        ScheduledStep step,
        DateOnly anchorDate,
        DateOnly asOf,
        IReadOnlyDictionary<ProcessStepId, ScheduledStep> byId,
        IReadOnlyDictionary<ProcessStepId, ProjectedStep> projected)
    {
        // What actually happened wins. A settled step's dates are facts, and a
        // projection that argued with them would be predicting the past.
        if (step.IsSettled)
        {
            var start = step.ActualStartOn ?? step.PlannedStartOn;
            var end = step.ActualEndOn ?? step.PlannedEndOn;

            return new ProjectedStep(step.Id, start, end, IsSettled: true);
        }

        var duration = step.PlannedEndOn.DayNumber - step.PlannedStartOn.DayNumber;

        if (step.Status == ProcessStepStatus.InProgress)
        {
            var started = step.ActualStartOn ?? step.PlannedStartOn;

            return new ProjectedStep(
                step.Id,
                started,
                Later(started.AddDays(duration), asOf),
                IsSettled: false);
        }

        // The step's own offset, recovered from the plan: how long after its
        // predecessors finished — or after the anchor — it was scheduled to begin.
        var plannedBase = step.Predecessors.Count == 0
            ? anchorDate
            : step.Predecessors.Max(x => byId[x].PlannedEndOn).AddDays(1);

        var offset = step.PlannedStartOn.DayNumber - plannedBase.DayNumber;

        var projectedBase = step.Predecessors.Count == 0
            ? anchorDate
            : step.Predecessors.Max(x => projected[x].ProjectedEndOn).AddDays(1);

        // Not started, so it cannot have started yesterday.
        var projectedStart = Later(projectedBase.AddDays(offset), asOf);

        return new ProjectedStep(
            step.Id,
            projectedStart,
            projectedStart.AddDays(duration),
            IsSettled: false);
    }

    private static DateOnly Later(DateOnly left, DateOnly right)
        => left > right ? left : right;

    private static IReadOnlyCollection<ScheduledStep> Adapt(ProcessPlan plan)
        => [.. plan.Steps.Select(step => new ScheduledStep(
            step.Id,
            step.Code,
            step.PlannedStartOn,
            step.PlannedEndOn,
            step.ActualStartOn,
            step.ActualEndOn,
            step.CurrentStatus,
            [.. step.Predecessors.Select(x => x.PredecessorStepId)]))];
}

/// <summary>
/// The facts a projection needs about one step, independent of where they came
/// from.
/// </summary>
/// <remarks>
/// <b>It exists so that a query handler never loads an aggregate</b>
/// ([ADR-016](../../../../../docs/adr/ADR-016-persistence-access-model.md)). The
/// impact read projects straight from <c>RegOSDbContext</c> into these; the
/// domain tests hand over a real <c>ProcessPlan</c>. Both reach the same walk,
/// which is what stops a second copy of the arithmetic appearing in the
/// application layer.
/// </remarks>
public sealed record ScheduledStep(
    ProcessStepId Id,
    string Code,
    DateOnly PlannedStartOn,
    DateOnly PlannedEndOn,
    DateOnly? ActualStartOn,
    DateOnly? ActualEndOn,
    ProcessStepStatus Status,
    IReadOnlyCollection<ProcessStepId> Predecessors)
{
    public bool IsSettled
        => Status is ProcessStepStatus.Complete or ProcessStepStatus.Skipped;
}

/// <summary>One step's dates as they now look. Never persisted (I8).</summary>
public sealed record ProjectedStep(
    ProcessStepId StepId,
    DateOnly ProjectedStartOn,
    DateOnly ProjectedEndOn,
    bool IsSettled);

/// <param name="SlipDays">
/// How much later the plan now finishes than it was scheduled to. <b>Zero when
/// the delay fell inside somebody's slack</b> — which is the answer a naive
/// "the late step is nine days late, so the plan is" would get wrong.
/// </param>
public sealed record PlanProjection(
    DateOnly? PlannedFinishOn,
    DateOnly? ProjectedFinishOn,
    int SlipDays,
    IReadOnlyDictionary<ProcessStepId, ProjectedStep> Steps);
